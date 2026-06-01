using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ReportsView : Form
{
    private readonly SnackLogic _snackLogic;
    private readonly BookingLogic _bookingLogic;
    private readonly BookingSnackLogic _bookingSnackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private Location _location;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public ReportsView(SnackLogic snackLogic, BookingLogic bookingLogic, BookingSnackLogic bookingSnackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _bookingLogic = bookingLogic;
        _bookingSnackLogic = bookingSnackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Location Location)
    {
        _user = user;
        _location = Location;
        InitializeForm();
    }

    private void InitializeForm()
    {
        var table = new Table(
            l10n("Auditorium"),
            l10n("TotalPrice"),
            l10n("Total from Seats"),
            l10n("Total from Snacks"),
            l10n("Snacks Sold"),
            l10n("Seats Reserved")
        );
        int ReservedSeatsQuantityCinema = 0;
        int SnacksSoldCinema = 0;
        decimal totalPriceCinema = 0m;
        decimal totalPriceSeatsCinema = 0m;
        decimal totalPriceSnacksCinema = 0m;
        Label(l10n($"Cinema - {_location.Name}"));
        Divider();
        foreach (Auditorium auditorium in _location.Auditoriums) {

            decimal totalPriceSeats = 0;
            decimal totalPriceAuditorium = 0m;
            decimal totalPriceSnacksAuditorium = 0m;
            int SnacksSoldAuditorium = 0;
            int ReservedSeatsQuantityAuditorium = 0;

            List<Booking> bookings = _bookingLogic.GetBookingsByAuditoriumID(auditorium.Id);
            foreach (Booking booking in bookings){
                List<string> seats = booking.Seats
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                ReservedSeatsQuantityAuditorium += seats.Count;
                ReservedSeatsQuantityCinema += ReservedSeatsQuantityAuditorium;

                totalPriceSeats += booking.TotalPrice;
                totalPriceSeatsCinema += totalPriceSeats;

                List<BookingSnack> bookedSnacks = _bookingSnackLogic.GetAllByBookingId(booking.Id);
                totalPriceSnacksAuditorium += bookedSnacks
                .Where(bs => bs.Snack != null)
                .Sum(bookedSnack => bookedSnack.Snack.Price * bookedSnack.BookedQuantity );
                SnacksSoldAuditorium = bookedSnacks.Sum(
                    bookedSnack => bookedSnack.BookedQuantity
                );
                SnacksSoldCinema += SnacksSoldAuditorium;
                totalPriceSnacksCinema += totalPriceSnacksAuditorium;

                totalPriceAuditorium += (booking.TotalPrice + totalPriceSnacksAuditorium);
                totalPriceCinema += totalPriceAuditorium;
            }
            table.AddRow(
                auditorium.Name,
                totalPriceAuditorium,
                totalPriceSeats,
                totalPriceSnacksAuditorium,
                SnacksSoldAuditorium,
                ReservedSeatsQuantityAuditorium
            );

        }

        Divider();
        Label(l10n($"Total from Snacks - {totalPriceSnacksCinema}"));
        Label(l10n($"Snacks sold - {SnacksSoldCinema}"));
        Label(l10n($"Total Price seats - {totalPriceSeatsCinema}"));
        Label(l10n($"Total Price Cinema - {totalPriceCinema}"));





        Add(table);
        Divider();
        Message(() => _statusMessage);
        var backButton = Button(l10n("Back")).OnClick(NavigateToMain);
        backButton.Hidden(() => _confirmingDelete);
}



    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

}
