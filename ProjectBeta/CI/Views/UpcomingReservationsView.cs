using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class UpcomingReservationsView : Form
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

    public UpcomingReservationsView(BookingLogic bookingLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user)
    {
        _user = user;
        InitializeForm();
    }

    private void InitializeForm()
    {
        List<Booking> reservations = _bookingLogic.GetBookingsByUserId(_user.Id);
        Heading(l10n("reservations.upcoming.heading"));
        Label(l10n("reservations.upcoming.instructions"));
        var table = new Table<Booking>(
            l10n("reservations.upcoming.table.movie"),
            l10n("reservations.upcoming.table.auditorium"),
            l10n("reservations.upcoming.table.seats"),
            l10n("reservations.upcoming.table.date"),
            l10n("reservations.upcoming.table.paid"),
            l10n("reservations.upcoming.table.total_price")
        )
        .EmptyMessage(l10n("reservations.upcoming.empty"))
        .OnSelect(OnBookingSelected);

        foreach (var reservation in reservations)
        {
            if (reservation.CreatedAt >= DateTime.Now)
            {
                table.AddRow(
                    reservation,
                    reservation.Movie,
                    reservation.AuditoriumId,
                    reservation.Seats,
                    reservation.CreatedAt,
                    reservation.Paid,
                    reservation.TotalPrice
                );
            }
        }

        Add(table);
        Divider();
        Message(() => _statusMessage);

        Button(l10n("reservations.upcoming.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }


    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
    private void OnBookingSelected(Booking booking)
    {
        Console.Clear();
        var reservationEditView = _serviceProvider.GetRequiredService<ReservationEditView>();
        reservationEditView.SetView(_user, booking);
        _appLoop.Display(reservationEditView);
    }
}
