using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReservationHistoryView : Form
{
    private readonly BookingLogic _bookingLogic;
    private readonly BookingSnackLogic _bookingSnackLogic;
    private readonly SnackLogic _snackLogic;
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

    public ReservationHistoryView(BookingLogic bookingLogic, BookingSnackLogic bookingSnackLogic, SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _bookingSnackLogic = bookingSnackLogic;
        _snackLogic = snackLogic;
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
        Heading(l10n("reservations.history.heading"));
        Label(l10n("reservations.history.instructions"));
        var table = new Table<Booking>(
            l10n("reservations.history.table.movie"),
            l10n("reservations.history.table.auditorium"),
            l10n("reservations.history.table.seats"),
            l10n("reservations.history.table.date"),
            l10n("reservations.history.table.paid"),
            l10n("reservations.history.table.snacks"),
            l10n("reservations.history.table.total_price")
        )
        .EmptyMessage(l10n("reservations.history.empty"))
        .OnSelect(OnBookingSelected);

        foreach (var reservation in reservations)
        {
            if (reservation.CreatedAt <= DateTime.Now)
            {
                var bookedSnacks = _bookingSnackLogic.GetAllByBookingId(reservation.Id);
                string snackNames = GetSnackNames(bookedSnacks);
                decimal totalPrice = CalculateTotalPrice(reservation, bookedSnacks);
                table.AddRow(
                    reservation,
                    reservation.Movie,
                    reservation.AuditoriumId,
                    reservation.Seats,
                    reservation.CreatedAt,
                    reservation.Paid,
                    snackNames,
                    totalPrice.ToString("F2")
                );
            }
        }

        Add(table);
        Divider();
        Message(() => _statusMessage);

        Button(l10n("reservations.history.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }



    private decimal CalculateTotalPrice(Booking booking, List<BookingSnack> bookedSnacks)
    {
        decimal totalPrice = booking.TotalPrice;
        foreach (var bookedSnack in bookedSnacks)
        {
            Snack snack = _snackLogic.GetById(bookedSnack.SnackId);
            totalPrice += snack.Price * bookedSnack.BookedQuantity;
        }
        return Math.Round(totalPrice, 2, MidpointRounding.AwayFromZero);
    }

    private string GetSnackNames(List<BookingSnack> bookedSnacks)
    {
        var snackNames = new List<string>();
        foreach (var bookedSnack in bookedSnacks)
        {
            Snack snack = _snackLogic.GetById(bookedSnack.SnackId);
            snackNames.Add($"{snack.Name} x {bookedSnack.BookedQuantity}");
        }
        return snackNames.Count > 0 ? string.Join(", ", snackNames) : "-";
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
