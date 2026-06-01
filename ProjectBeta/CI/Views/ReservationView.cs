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
    private readonly LocationLogic _locationLogic;
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

    public ReservationView(BookingLogic bookingLogic, PricingLogic pricingLogic, LocationLogic locationLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _pricingLogic = pricingLogic;
        _locationLogic = locationLogic;
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
        var backButton = Button(l10n("reservations.create.actions.back")).OnClick(NavigateToLocation);
        backButton.Hidden(() => _confirmingDelete);

        Navigation(
            Button(l10n("reservations.create.actions.save")).OnClick(form => OnSave(form, _ageInputs, _userSeatSelect!)),
            backButton);
    }

    private static List<int?> BuildSeatAges(List<NumberInput> inputs)
        => inputs.Select(n => n.EffectiveValue.HasValue ? (int?)((int)n.EffectiveValue.Value) : null).ToList();

    private static string SerializeSeatAges(List<int?> ages)
        => string.Join(",", ages.Select(a => a.HasValue ? a.Value.ToString() : string.Empty));

    private string BuildPricingSummary(List<NumberInput> ageInputs)
    {
        var seatAges = BuildSeatAges(ageInputs);
        var pricing = _pricingLogic.CalculatePricing(_seatTypes, seatAges, _movie.ScheduleDate.ToDateTime(_movie.StartTime));

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < _reservedSeats.Count && i < pricing.SeatLines.Count; i++)
        {
            string seat = _reservedSeats[i];
            var line = pricing.SeatLines[i];
            if (line.Discount != null)
            {
                string discountResult = $"{seat,-6} €{line.BasePrice:F2} → ";
                discountResult += $"€{line.FinalPrice:F2}  ({line.Discount.Name} {line.Discount.Percentage}%)";
                sb.AppendLine(discountResult);
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

        var pricing = _pricingLogic.CalculatePricing(_seatTypes, seatAges, startDateTime);

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
        NavigateToLocation();
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

        var pricing = _pricingLogic.CalculatePricing(_seatTypes, seatAges, startDateTime);

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

    private void NavigateToLocation()
    {
        Console.Clear();
        var location = _locationLogic.GetById(_locationId);
        if (location != null)
        {
            var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
            detailView.SetView(_user, location);
            _appLoop.Display(detailView);
        }
        else
        {
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        }
    }
}
