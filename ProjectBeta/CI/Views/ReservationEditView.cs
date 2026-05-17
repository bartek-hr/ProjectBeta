using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReservationEditView : Form
{
    private readonly BookingLogic _bookingLogic;
    private readonly BookingSnackLogic _bookingSnackLogic;
    private readonly SnackLogic _snackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private List<BookingSnack> _bookedSnacks = new();
    private User _user;
    private Booking _booking;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public ReservationEditView(BookingLogic bookingLogic, BookingSnackLogic bookingSnackLogic, SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
        _bookingSnackLogic = bookingSnackLogic;
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Booking booking)
    {
        _user = user;
        _booking = booking;
        _bookedSnacks = _bookingSnackLogic.GetAllByBookingId(booking.Id);
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("reservations.edit.heading"));
        Label(l10n("reservations.edit.instructions"));
        var table = new Table(
            l10n("reservations.edit.table.movie"),
            l10n("reservations.edit.table.auditorium"),
            l10n("reservations.edit.table.seats"),
            l10n("reservations.edit.table.date"),
            l10n("reservations.edit.table.paid"),
            l10n("Snacks"),
            l10n("reservations.edit.table.total_price")
        )
        .EmptyMessage(l10n("reservations.edit.empty"));
        decimal totalPrice = CalculateTotalPrice();
        string snackNames = GetSnackNames();
        table.AddRow(
            _booking.Movie,
            _booking.AuditoriumId,
            _booking.Seats,
            _booking.CreatedAt,
            _booking.Paid,
            snackNames,
            totalPrice
        );
        

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Button(l10n("reservations.edit.actions.delete")).OnClick(OnDelete);
        Button(l10n("reservations.edit.actions.pay")).OnClick(OnPay);

        Button(l10n("reservations.edit.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }

    private decimal CalculateTotalPrice()
    {
        decimal totalPrice = _booking.TotalPrice;
        foreach(BookingSnack bookedSnack in _bookedSnacks) {
            Snack snack = _snackLogic.GetById(bookedSnack.SnackId);
            totalPrice += snack.Price * bookedSnack.BookedQuantity;
        }
        return totalPrice;
    }
    private string GetSnackNames()
    {
        List<string> snackNames = new();
        foreach(BookingSnack bookedSnack in _bookedSnacks) {
            Snack snack = _snackLogic.GetById(bookedSnack.SnackId);
            snackNames.Add($"{snack.Name} x {bookedSnack.BookedQuantity}");
        }
        return string.Join(", ", snackNames);
    }

    private void OnDelete(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        _bookingLogic.DeleteBooking(_booking.Id);
        NavigateToMain();

    }

    private void OnPay(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;
        _bookingLogic.MarkAsPaid(_booking.Id);
        NavigateToReceiptView();
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
    private void NavigateToReceiptView()
    {
        Console.Clear();
        var receiptView = _serviceProvider.GetRequiredService<ReceiptView>();
        receiptView.SetView(_user, _booking);
        _appLoop.Display(receiptView);
    }
}
