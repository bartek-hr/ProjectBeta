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
        Heading(l10n("receipts.heading"));
        Label(l10n("receipts.cinema"));
        Divider();

        foreach (BookingSnack bookedSnack in bookedSnacks)
        {
            string line = string.Format(
                "{0,-25} {1,5:0.00} x {2,-3} {3,8:0.00}",
                bookedSnack.Snack.Name,
                bookedSnack.Snack.Price,
                bookedSnack.BookedQuantity,
                bookedSnack.Snack.Price * bookedSnack.BookedQuantity
            );
            Label(line);
            totalPrice += bookedSnack.Snack.Price * bookedSnack.BookedQuantity;
        }

        foreach (string seat in seats)
        {
            Label($"  {seat}");
        }
        Label(string.Format("{0,-38} {1,8:0.00}", l10n("receipts.seats_total"), _booking.TotalPrice));

        Divider();
        Label(string.Format("{0,-38} {1,8:0.00}", l10n("receipts.total"), totalPrice));
        Divider();
        Label(l10n("receipts.thank_you"));

        Message(() => _statusMessage);
        Navigation(
            Button(l10n("receipts.actions.back")).OnClick(NavigateToMain)
        );
    }



    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

}
