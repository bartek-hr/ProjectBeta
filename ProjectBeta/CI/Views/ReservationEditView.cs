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
        Heading(l10n("reservations.edit.heading"));
        Label(l10n("reservations.edit.instructions"));
        var table = new Table(
            l10n("reservations.edit.table.movie"),
            l10n("reservations.edit.table.auditorium"),
            l10n("reservations.edit.table.seats"),
            l10n("reservations.edit.table.date"),
            l10n("reservations.edit.table.paid"),
            l10n("reservations.edit.table.total_price")
        )
        .EmptyMessage(l10n("reservations.edit.empty"));

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
        Button(l10n("reservations.edit.actions.delete")).OnClick(OnDelete);
        Button(l10n("reservations.edit.actions.pay")).OnClick(OnPay);

        Button(l10n("reservations.edit.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
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
