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
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private Booking _booking;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public ReservationEditView(BookingLogic bookingLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingLogic = bookingLogic;
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
        Heading("Reservation Details");
        Label("Tab to navigate, Shift+Tab to go back.");
        var table = new Table(
            "Movie", "Auditorium", "Seats", "Date", "Paid", "Total Price"
        )
        .EmptyMessage("No movies scheduled.");

        table.AddRow(
            _booking.Movie,
            _booking.AuditoriumId,
            _booking.Seats,
            _booking.CreatedAt,
            _booking.Paid,
            _booking.TotalPrice
        );
        

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Button("Delete").OnClick(OnDelete);
        Button("Pay").OnClick(OnPay);

        Button("Back").OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
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
