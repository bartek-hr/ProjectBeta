using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public record AppliedDiscount(int Id, string Name, decimal Percentage);

// Per-seat pricing line: base price, final price
// (after best-match discount), and which discount was applied (null if none).
public record SeatPricingLine(decimal BasePrice, decimal FinalPrice, AppliedDiscount? Discount, string? SubscriptionNote = null);

// Subscription data needed by PricingLogic to apply seat discounts at booking time.
public record SubscriptionPricingContext(
    int? ApplicableDayOfWeek,
    decimal SubscriptionSeatPrice
);

public record PricingResult(
    decimal BasePrice,
    decimal FinalPrice,
    IReadOnlyList<AppliedDiscount> Discounts,
    IReadOnlyList<SeatPricingLine> SeatLines
);

public class PricingLogic
{
    private readonly DiscountAccess _discountAccess;
    private readonly SeatPriceAccess _seatAccess;

    public PricingLogic(DiscountAccess discountAccess, SeatPriceAccess seatAccess)
    {
        _discountAccess = discountAccess;
        _seatAccess = seatAccess;
    }

    // Calculates the price per seat based on active discount rules.
    // Each seat gets at most one discount — the best matching one wins.
    // Age discounts only apply if an age was given for that seat.
    // Group/day discounts apply to all seats when the condition is met.
    public PricingResult CalculatePricing(
        List<int> seatTypes,
        List<int?> seatAges,
        DateTime screeningDate,
        int? userSeatIndex = null,
        SubscriptionPricingContext? subscription = null)
    {
        var activeDiscounts = _discountAccess.GetActive();
        var seatPrices = _seatAccess.GetAll().ToDictionary(st => st.Id, st => st.Price);
        decimal basePrice = seatTypes.Sum(t => seatPrices.GetValueOrDefault(t, 0m));

        var appliedDiscounts = new List<AppliedDiscount>();
        var seatLines = new List<SeatPricingLine>();
        decimal finalPrice = 0m;

        for (int i = 0; i < seatTypes.Count; i++)
        {
            int seatType = seatTypes[i];
            int? seatAge = i < seatAges.Count ? seatAges[i] : null;
            decimal seatBase = seatPrices.GetValueOrDefault(seatType, 0m);

            // Apply subscription to the user's own seat when the day matches.
            if (i == userSeatIndex && subscription != null)
            {
                bool dayMatches = !subscription.ApplicableDayOfWeek.HasValue ||
                    (int)screeningDate.DayOfWeek == subscription.ApplicableDayOfWeek.Value;

                if (dayMatches)
                {
                    decimal afterSub = seatBase <= subscription.SubscriptionSeatPrice
                        ? 0m
                        : seatBase - subscription.SubscriptionSeatPrice;

                    string note = seatBase <= subscription.SubscriptionSeatPrice
                        ? "Subscription (free)"
                        : $"Subscription (−€{subscription.SubscriptionSeatPrice:F2})";

                    finalPrice += afterSub;
                    seatLines.Add(new SeatPricingLine(seatBase, afterSub, null, note));
                    continue;
                }
            }

            var candidates = new List<(int Id, string Name, decimal Pct)>();

            foreach (var discount in activeDiscounts)
            {
                if (IsDiscountEligible(discount, seatAge, seatTypes.Count, screeningDate))
                {
                    candidates.Add((discount.Id, discount.Name, discount.Percentage));
                }
            }

            if (candidates.Count > 0)
            {
                var best = candidates.MaxBy(c => c.Pct);
                var applied = new AppliedDiscount(best.Id, best.Name, best.Pct);
                appliedDiscounts.Add(applied);
                decimal seatFinal = seatBase * (1 - best.Pct / 100m);
                finalPrice += seatFinal;
                seatLines.Add(new SeatPricingLine(seatBase, seatFinal, applied));
            }
            else
            {
                finalPrice += seatBase;
                seatLines.Add(new SeatPricingLine(seatBase, seatBase, null));
            }
        }

        var distinctDiscounts = appliedDiscounts.DistinctBy(d => d.Name).ToList();
        return new PricingResult(basePrice, finalPrice, distinctDiscounts, seatLines);
    }

    private static bool IsDiscountEligible(
        Discount discount,
        int? seatAge,
        int seatCount,
        DateTime screeningDate)
    {
        // Day-of-week discounts:
        if (discount.RequiredDayOfWeek.HasValue)
        {
            if (screeningDate.DayOfWeek != (DayOfWeek)discount.RequiredDayOfWeek.Value)
                return false;
            // Day discount has no further age/group requirements — it applies.
            if (!discount.MaxAge.HasValue && !discount.MinAge.HasValue && !discount.MinGroupSize.HasValue)
                return true;
        }

        bool eligible = false;

        // Age-based discounts: only apply when an age was provided for this seat.
        if (discount.MaxAge.HasValue && seatAge.HasValue)
        {
            eligible = eligible || seatAge.Value <= discount.MaxAge.Value;
        }
        if (discount.MinAge.HasValue && seatAge.HasValue)
        {
            eligible = eligible || seatAge.Value > discount.MinAge.Value;
        }

        // Group discounts: apply to all seats in the booking when group size is met.
        if (discount.MinGroupSize.HasValue)
        {
            eligible = eligible || seatCount >= discount.MinGroupSize.Value;
        }

        return eligible;
    }

    public static int ComputeAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
            age--;
        return age;
    }
}
