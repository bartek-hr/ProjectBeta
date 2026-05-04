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

    public ReservationHistoryView(BookingLogic bookingLogic, AppLoop appLoop, IServiceProvider serviceProvider)
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
        Heading("Reservation Details");
        Label("Tab to navigate, Shift+Tab to go back.");
        var table = new Table<Booking>(
            "Movie", "Auditorium", "Seats", "Date", "Paid", "Total Price"
        )
        .EmptyMessage("No movies scheduled.")
        .OnSelect(OnBookingSelected);

        foreach (var reservation in reservations)
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

        Add(table);
        Divider();
        Message(() => _statusMessage);

        Button("Back").OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
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
