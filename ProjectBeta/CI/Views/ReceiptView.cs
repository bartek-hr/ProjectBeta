using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReceiptView : Form
{
    private readonly SnackLogic _snackLogic;
    private readonly BookingSnackLogic _bookingSnackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private Booking _booking;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public ReceiptView(SnackLogic snackLogic, BookingSnackLogic bookingSnackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _bookingSnackLogic = bookingSnackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Booking booking)
    {
        _user = user;
        _booking = booking;
        InitializeForm();
    }

    private void InitializeForm()
{
    List<BookingSnack> bookedSnacks = _bookingSnackLogic.GetAllByBookingId(_booking.Id);

    List<string> seats = _booking.Seats
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .ToList();

    decimal totalPrice = _booking.TotalPrice;

    Divider();
    Label(l10n("Receipt"));
    Label(l10n("Cinema 1"));
    Divider();

    // Snacks
    foreach (BookingSnack bookedSnack in bookedSnacks)
    {
        string line = string.Format(
            "{0,-25} {1,5:0.00} x {2,-3} {3,8:0.00}", 
            bookedSnack.Snack.Name, 
            bookedSnack.Snack.Price, 
            bookedSnack.BookedQuantity, 
            bookedSnack.Snack.Price * bookedSnack.BookedQuantity
        );
        Label(l10n(line));
        totalPrice += bookedSnack.Snack.Price * bookedSnack.BookedQuantity;
    }

    // Seats
    foreach (string seat in seats)
    {
        Label(l10n($"Seat {seat}"));
    }
    Label(l10n(string.Format("{0,-38} {1,8:0.00}", "Seats Total", _booking.TotalPrice)));

    Divider();

    // Total
    Label(l10n(string.Format("{0,-38} {1,8:0.00}", "Total", totalPrice)));

    Divider();
    Label(l10n("Thank You!"));

    Message(() => _statusMessage);

    Button(l10n("Back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
}



    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

}
