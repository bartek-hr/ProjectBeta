using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Rendering;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReservationView : Form
{
    private readonly BookingLogic _bookingLogic;
    private readonly PricingLogic _pricingLogic;
    private readonly SubscriptionLogic _subscriptionLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private MovieSchedule _movie;
    private List<string> _reservedSeats;
    private string? _statusMessage;
    private List<int> _seatTypes;
    private int _auditoriumId;
    private int _locationId;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;
    private List<NumberInput> _ageInputs = new();
    private Select? _userSeatSelect;
    private SubscriptionPricingContext? _subscriptionInfo;

    public ReservationView(BookingLogic bookingLogic, PricingLogic pricingLogic, SubscriptionLogic subscriptionLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _pricingLogic = pricingLogic;
        _subscriptionLogic = subscriptionLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, MovieSchedule movie, List<string> reservedSeats, List<int> seatTypes, int auditoriumId, int locationId)
    {
        _user = user;
        _movie = movie;
        _locationId = locationId;
        _reservedSeats = reservedSeats;
        _seatTypes = seatTypes;
        _auditoriumId = auditoriumId;
        InitializeForm();
    }

    private void InitializeForm()
    {
        _subscriptionInfo = _subscriptionLogic.GetActiveSubscriptionPricingInfo(_user.Id);
        Heading(l10n("reservations.create.heading"));
        Label(l10n("reservations.create.instructions"));
        Divider();
        var table = new Table(
            l10n("reservations.create.table.movie"),
            l10n("reservations.create.table.seat"),
            l10n("reservations.create.table.date"),
            l10n("reservations.create.table.start"),
            l10n("reservations.create.table.end"),
            l10n("reservations.create.table.auditorium"),
            l10n("reservations.create.table.snacks")
        );

        foreach (var reservedSeat in _reservedSeats)
        {
            table.AddRow(
                _movie.Movie.Title,
                reservedSeat,
                _movie.ScheduleDate.ToString("yyyy-MM-dd"),
                _movie.StartTime.ToString("HH:mm"),
                _movie.EndTime.ToString("HH:mm"),
                $"{_auditoriumId}",
                l10n("reservations.create.values.no_snacks")
            );
        }

        Add(table);
        Divider();

        // Optional: which seat does the booking user personally sit in?
        var userSeatSelect = Select(l10n("reservations.create.user_seat_label"));
        userSeatSelect.AddOption(l10n("reservations.create.user_seat_none"));
        foreach (var s in _reservedSeats)
            userSeatSelect.AddOption(s);

        // Per-seat age inputs — optional,
        // If the user selected their own seat, that input is pre-filled
        Label(l10n("reservations.create.age_prompt"));
        int userAge = PricingLogic.ComputeAge(_user.DateOfBirth);
        _ageInputs = new List<NumberInput>();
        for (int i = 0; i < _reservedSeats.Count; i++)
        {
            string seat = _reservedSeats[i];
            var input = NumberInput(l10n("reservations.create.age_label", new Dictionary<string, string> { ["seat"] = seat }))
                .Min(0).Max(130)
                .ReadOnly(() => userSeatSelect.Value == seat, userAge);
            _ageInputs.Add(input);
        }
        _userSeatSelect = userSeatSelect;

        Divider();
        Add(new Message(() => BuildPricingSummary(_ageInputs), Style.Default));
        Message(() => _statusMessage);
        var backButton = Button(l10n("reservations.create.actions.back")).OnClick(NavigateToMain);
        backButton.Hidden(() => _confirmingDelete);

        Navigation(
            Button(l10n("reservations.create.actions.save")).OnClick(form => OnSave(form, _ageInputs, _userSeatSelect!)),
            Button("Snacks").OnClick(form => OnSnacks(form)),
            backButton);
    }

    private (int? Index, SubscriptionPricingContext? Ctx) ResolveUserSeat(string? seatLabel)
    {
        if (string.IsNullOrEmpty(seatLabel)) return (null, null);
        int idx = _reservedSeats.IndexOf(seatLabel);
        return idx >= 0 ? (idx, _subscriptionInfo) : (null, null);
    }

    private static List<int?> BuildSeatAges(List<NumberInput> inputs)
        => inputs.Select(n => n.EffectiveValue.HasValue ? (int?)((int)n.EffectiveValue.Value) : null).ToList();

    private static string SerializeSeatAges(List<int?> ages)
        => string.Join(",", ages.Select(a => a.HasValue ? a.Value.ToString() : string.Empty));

    private string BuildPricingSummary(List<NumberInput> ageInputs)
    {
        var seatAges = BuildSeatAges(ageInputs);

        string? selectedSeat = _userSeatSelect?.Value == l10n("reservations.create.user_seat_none")
            ? null : _userSeatSelect?.Value;
        var (userSeatIdx, subCtx) = ResolveUserSeat(selectedSeat);

        var pricing = _pricingLogic.CalculatePricing(
            _seatTypes, seatAges, _movie.ScheduleDate.ToDateTime(_movie.StartTime), userSeatIdx, subCtx);

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _reservedSeats.Count && i < pricing.SeatLines.Count; i++)
        {
            string seat = _reservedSeats[i];
            var line = pricing.SeatLines[i];
            if (line.SubscriptionNote != null)
            {
                sb.AppendLine($"{seat,-6} €{line.BasePrice:F2} → €{line.FinalPrice:F2}  ({line.SubscriptionNote})");
            }
            else if (line.Discount != null)
            {
                sb.AppendLine($"{seat,-6} €{line.BasePrice:F2} → €{line.FinalPrice:F2}  ({line.Discount.Name} {line.Discount.Percentage}%)");
            }
            else
            {
                sb.AppendLine($"{seat,-6} €{line.BasePrice:F2}");
            }
        }
        sb.Append($"Total:  €{pricing.FinalPrice:F2}");
        return sb.ToString();
    }

    private void OnSave(Form form, List<NumberInput> ageInputs, Select userSeatSelect)
    {
        _fieldErrors = null;
        _statusMessage = null;

        string reservedseats = string.Join(",", _reservedSeats);
        DateTime startDateTime = _movie.ScheduleDate.ToDateTime(_movie.StartTime);
        var seatAges = BuildSeatAges(ageInputs);
        string? userSeat = userSeatSelect.Value == l10n("reservations.create.user_seat_none")
            ? null
            : userSeatSelect.Value;

        var (userSeatIdx, subCtx) = ResolveUserSeat(userSeat);
        var pricing = _pricingLogic.CalculatePricing(_seatTypes, seatAges, startDateTime, userSeatIdx, subCtx);

        _bookingLogic.CreateBooking(
            _user.Id,
            pricing.FinalPrice,
            pricing.BasePrice,
            _auditoriumId,
            reservedseats,
            SerializeSeatAges(seatAges),
            userSeat,
            $"{_movie.Movie.Title}",
            startDateTime,
            pricing.Discounts.Select(d => d.Id)
        );
        NavigateToMain();
    }
    private void OnSnacks(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        string reservedseats = string.Join(",", _reservedSeats);
        DateTime startDateTime = _movie.ScheduleDate.ToDateTime(_movie.StartTime);
        var seatAges = BuildSeatAges(_ageInputs);
        string? userSeat = _userSeatSelect?.Value == l10n("reservations.create.user_seat_none")
            ? null
            : _userSeatSelect?.Value;

        var (userSeatIdx, subCtx) = ResolveUserSeat(userSeat);
        var pricing = _pricingLogic.CalculatePricing(_seatTypes, seatAges, startDateTime, userSeatIdx, subCtx);

        Booking createdBooking = _bookingLogic.CreateBooking(
            _user.Id,
            pricing.FinalPrice,
            pricing.BasePrice,
            _auditoriumId,
            reservedseats,
            SerializeSeatAges(seatAges),
            userSeat,
            $"{_movie.Movie.Title}",
            startDateTime,
            pricing.Discounts.Select(d => d.Id)
        );
        NavigateToBookingSnacksView(createdBooking);
    }
    private void NavigateToBookingSnacksView(Booking createdBooking)
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<BookingSnacksView>();
        mainView.SetView(_user, createdBooking, _locationId);
        _appLoop.Display(mainView);
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
}
