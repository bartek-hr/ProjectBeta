using ProjectBeta.CI.Components;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
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

        var actionButtons = new List<Button>
        {
            Button("Locations").OnClick(() =>
            {
                Console.Clear();
                var locationView = _serviceProvider.GetRequiredService<LocationView>();
                locationView.SetUser(_user);
                _appLoop.Display(locationView);
            }),
Button(l10n("main.dashboard.actions.reservation_history")).OnClick(() =>
            {
                Console.Clear();
                var reservationHistoryView = _serviceProvider.GetRequiredService<ReservationHistoryView>();
                reservationHistoryView.SetView(_user);
                _appLoop.Display(reservationHistoryView);
            }),
            Button(l10n("main.dashboard.actions.upcoming_reservations")).OnClick(() =>
            {
                Console.Clear();
                var upcomingReservationsView = _serviceProvider.GetRequiredService<UpcomingReservationsView>();
                upcomingReservationsView.SetView(_user);
                _appLoop.Display(upcomingReservationsView);
            }),
            Button(l10n("main.dashboard.actions.account_details")).OnClick(() =>
            {
                Console.Clear();
                var accountView = _serviceProvider.GetRequiredService<AccountView>();
                accountView.SetUser(_user);
                _appLoop.Display(accountView);
            })
        };
        if (!_user.IsAdmin())
        {
            actionButtons.Add(Button(l10n("main.dashboard.actions.my_subscription")).OnClick(() =>
            {
                Console.Clear();
                var userSubscriptionView = _serviceProvider.GetRequiredService<UserSubscriptionView>();
                userSubscriptionView.SetUser(_user);
                _appLoop.Display(userSubscriptionView);
            }));
        }

        if (_user.IsAdmin())
        {
            actionButtons.Add(Button(l10n("main.dashboard.actions.users")).OnClick(() =>
            {
                Console.Clear();
                var usersView = _serviceProvider.GetRequiredService<UsersView>();
                usersView.SetUser(_user);
                _appLoop.Display(usersView);
            }));

            actionButtons.Add(Button(l10n("main.dashboard.actions.seat_prices")).OnClick(() =>
            {
                Console.Clear();
                var seatPriceView = _serviceProvider.GetRequiredService<SeatPriceView>();
                seatPriceView.SetUser(_user);
                _appLoop.Display(seatPriceView);
            }));

            actionButtons.Add(Button(l10n("main.dashboard.actions.discounts")).OnClick(() =>
            {
                Console.Clear();
                var discountView = _serviceProvider.GetRequiredService<DiscountView>();
                discountView.SetUser(_user);
                _appLoop.Display(discountView);
            }));

            actionButtons.Add(Button(l10n("main.dashboard.actions.subscriptions")).OnClick(() =>
            {
                Console.Clear();
                var subscriptionView = _serviceProvider.GetRequiredService<SubscriptionView>();
                subscriptionView.SetUser(_user);
                _appLoop.Display(subscriptionView);
            }));

            actionButtons.Add(Button(l10n("main.dashboard.actions.manage_movies")).OnClick(() =>
            {
                Console.Clear();
                var manageMoviesView = _serviceProvider.GetRequiredService<ManageMoviesView>();
                manageMoviesView.SetUser(_user);
                _appLoop.Display(manageMoviesView);
            }));
        }

        Navigation(actionButtons.ToArray());
        Divider();
        Message(() => _statusMessage);
        LogoutButton(_appLoop, _serviceProvider);
    }
}
