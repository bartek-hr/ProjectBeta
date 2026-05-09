using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReservationView : Form
{
    private readonly BookingLogic _bookingLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private MovieSchedule _movie;
    private List<string> _reservedSeats;
    private string? _statusMessage;
    private List<int> _seatTypes;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public ReservationView(BookingLogic bookingLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, MovieSchedule movie, List<string> reservedSeats, List<int> seatTypes)
    {
        _user = user;
        _movie = movie;
        _reservedSeats = reservedSeats;
        _seatTypes = seatTypes;
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
                l10n("reservations.create.values.default_auditorium"),
                l10n("reservations.create.values.no_snacks")
            );
        }

        Add(table);
        Divider();
        Label(l10n("reservations.create.total_price", new Dictionary<string, string>
        {
            ["amount"] = _bookingLogic.DetermineTotalPrice(_seatTypes).ToString()
        }));
        Message(() => _statusMessage);
        Button(l10n("reservations.create.actions.save")).OnClick(OnSave);

        Button(l10n("reservations.create.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        string reservedseats = string.Join(",", _reservedSeats);
        DateTime startDateTime = _movie.ScheduleDate.ToDateTime(_movie.StartTime);

        _bookingLogic.CreateBooking(
            _user.Id,
            _bookingLogic.DetermineTotalPrice(_seatTypes),
            1,
            reservedseats,
            1,
            $"{_movie.Movie.Title}",
            startDateTime
        );
        NavigateToMain();
    }

  

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
}
