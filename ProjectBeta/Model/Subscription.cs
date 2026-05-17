namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class Subscription
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "validation.subscription.plan_id.required")]
    public int SubscriptionPlanId { get; set; }

    public SubscriptionPlan? SubscriptionPlan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "validation.subscription.primary_user_id.required")]
    public int PrimaryUserId { get; set; }

    public User? PrimaryUser { get; set; }

    public int? SharedWithUserId { get; set; }

    public User? SharedWithUser { get; set; }

    public bool IsShared { get; set; }

    public decimal MonthlyPrice { get; set; }

    public decimal DiscountPercentage { get; set; }

    public decimal FinalMonthlyPrice { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SubscriptionPurchase> Purchases { get; set; } = new();

    public void ApplySharedDiscount()
    {
        IsShared = SharedWithUserId.HasValue;
        DiscountPercentage = IsShared ? 10m : 0m;
        FinalMonthlyPrice = MonthlyPrice - (MonthlyPrice * DiscountPercentage / 100m);
    }

    public void RemoveSharedUser()
    {
        SharedWithUserId = null;
        ApplySharedDiscount();
    }

    public bool BelongsToUser(int userId)
    {
        return PrimaryUserId == userId || SharedWithUserId == userId;
    }
}
