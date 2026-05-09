using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Views;
using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectBeta.Logic;
namespace ProjectBeta.CI.Views;

public sealed class MainView : Form
{
    private User _user;
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private readonly AppLoop _appLoop;
    private Dictionary<string, string[]>? _fieldErrors;

    public MainView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _user = new User();
        _appLoop = appLoop;
        _statusMessage = null;
    }
    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("main.dashboard.heading", new Dictionary<string, string> { ["name"] = _user.Username }));
        Label(l10n("main.dashboard.email", new Dictionary<string, string> { ["email"] = _user.Email }));
        Label(l10n("main.dashboard.full_name", new Dictionary<string, string>
        {
            ["name"] = $"{_user.FirstName} {_user.LastName}"
        }));


        Button(l10n("main.dashboard.actions.movies")).OnClick(() => 
        {    Console.Clear();
            var moviesVies = _serviceProvider.GetRequiredService<MoviesView>();
            moviesVies.SetUser(_user);
            _appLoop.Display(moviesVies);
        });

        Button(l10n("main.dashboard.actions.reservation_history")).OnClick(() =>
        {
            Console.Clear();
            var reservationHistoryView = _serviceProvider.GetRequiredService<ReservationHistoryView>();
            reservationHistoryView.SetView(_user);
            _appLoop.Display(reservationHistoryView);
        });

        Button(l10n("main.dashboard.actions.upcoming_reservations")).OnClick(() =>
        {
            Console.Clear();
            var upcomingReservationsView = _serviceProvider.GetRequiredService<UpcomingReservationsView>();
            upcomingReservationsView.SetView(_user);
            _appLoop.Display(upcomingReservationsView);
        });
        Button(l10n("main.dashboard.actions.account_details")).OnClick(() =>
        {
            Console.Clear();
            var accountView = _serviceProvider.GetRequiredService<AccountView>();
            accountView.SetUser(_user);
            _appLoop.Display(accountView);
        });
        if (_user.Role == "Admin")
        {
            Button(l10n("main.dashboard.actions.reports")).OnClick(form => { _statusMessage = l10n("main.dashboard.status.reports_tbd"); });
            Button(l10n("main.dashboard.actions.users")).OnClick(() =>
            {
                Console.Clear();
                var usersView = _serviceProvider.GetRequiredService<UsersView>();
                usersView.SetUser(_user);
                _appLoop.Display(usersView);
            });
            Button(l10n("main.dashboard.actions.cinemas")).OnClick(() =>
            {
                Console.Clear();
                var cinemaView = _serviceProvider.GetRequiredService<CinemaView>();
                cinemaView.SetUser(_user);
                _appLoop.Display(cinemaView);
            });
        }
        Divider();
        Message(() => _statusMessage);
        LogoutButton(_appLoop, _serviceProvider);
    }
}
