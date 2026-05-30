using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.CI.Components;
using ProjectBeta.Localization;

namespace ProjectBeta.CI.Views;

public sealed class UserSubscriptionView : Form
{
    private readonly SubscriptionLogic _subscriptionLogic;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user = null!;
    private string? _statusMessage;
    private int? _selectedSubscriptionId;

    public UserSubscriptionView(SubscriptionLogic subscriptionLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _subscriptionLogic = subscriptionLogic;
        _serviceProvider = serviceProvider;
        _appLoop = appLoop;
    }

    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("user.subscriptions.heading"));
        Divider();

        var allSubs = _subscriptionLogic.GetAvailableSubscriptionsWithSeatPrice();
        var activeSub = allSubs.FirstOrDefault(s =>
            s.UserSubscriptions != null &&
            s.UserSubscriptions.Any(us => us.UserId == _user.Id && us.IsActive));

        if (activeSub != null)
        {
            var myUserSub = activeSub.UserSubscriptions.First(us => us.UserId == _user.Id && us.IsActive);
            var day = activeSub.ApplicableDayOfWeek.HasValue
                ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)activeSub.ApplicableDayOfWeek.Value)
                : l10n("user.subscriptions.any_day");

            Label(l10n("user.subscriptions.current_label"));
            Label($"  {activeSub.Name} — {l10n("user.subscriptions.price_label")}: {activeSub.Price:C}, {l10n("user.subscriptions.day_label")}: {day}, {l10n("user.subscriptions.seat_label")}: {activeSub.SeatPrice?.Name}");

            if (myUserSub.IsConnected && myUserSub.ConnectedWithUserId.HasValue)
            {
                var connectedEmail = _subscriptionLogic.GetUserEmail(myUserSub.ConnectedWithUserId.Value);
                var discountedPrice = activeSub.Price * (1 - activeSub.ConnectDiscount);
                Label($"  {l10n("user.subscriptions.connected_with")}: {connectedEmail}");
                Label($"  {l10n("user.subscriptions.connected_discount")}: {activeSub.ConnectDiscount * 100:0}% ({activeSub.Price:C} → {discountedPrice:C})");
            }
            else if (activeSub.IsConnectAllowed)
            {
                Divider();
                Label(l10n("user.subscriptions.connect.heading"));
                Label(l10n("user.subscriptions.connect.instructions"));
                var emailInput = TextInput(l10n("user.subscriptions.connect.email_label")).Key("connect_email");
                Navigation(
                    Button(l10n("user.subscriptions.connect.button")).OnClick(_ =>
                    {
                        try
                        {
                            _subscriptionLogic.ConnectSubscription(_user.Id, emailInput.Value?.Trim() ?? "");
                            _statusMessage = l10n("user.subscriptions.status.connected");
                        }
                        catch (InvalidOperationException ex)
                        {
                            _statusMessage = ex.Message;
                        }
                        SetUser(_user);
                    })
                );
            }

            Divider();
            Navigation(
                Button(l10n("user.subscriptions.actions.cancel")).OnClick(_ =>
                {
                    _subscriptionLogic.CancelSubscription(_user.Id);
                    _statusMessage = l10n("user.subscriptions.status.cancelled");
                    SetUser(_user);
                })
            );
        }
        else
        {
            Label(l10n("user.subscriptions.no_active"));
            Divider();

            var available = allSubs;
            if (available.Count == 0)
            {
                Label(l10n("user.subscriptions.no_available"));
            }
            else
            {
                Label(l10n("user.subscriptions.available_label"));

                if (!_selectedSubscriptionId.HasValue && available.Count > 0)
                    _selectedSubscriptionId = available[0].Id;

                var subButtons = new List<Button>();
                Button? selectedButton = null;
                foreach (var sub in available)
                {
                    var subscription = sub;
                    var isSelected = _selectedSubscriptionId == subscription.Id;
                    var day = subscription.ApplicableDayOfWeek.HasValue
                        ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)subscription.ApplicableDayOfWeek.Value)
                        : l10n("user.subscriptions.any_day");
                    var label = $"{(isSelected ? "> " : "  ")}{subscription.Name} — {l10n("user.subscriptions.price_label")}: {subscription.Price:C}, {l10n("user.subscriptions.day_label")}: {day}, {l10n("user.subscriptions.seat_label")}: {subscription.SeatPrice?.Name}";
                    var button = Button(label).OnClick(_ =>
                    {
                        _selectedSubscriptionId = subscription.Id;
                        _statusMessage = null;
                        SetUser(_user);
                    });
                    subButtons.Add(button);
                    if (isSelected) selectedButton = button;
                }
                var nav = Navigation(subButtons.ToArray());
                if (selectedButton != null) nav.SetActive(selectedButton);

                if (_selectedSubscriptionId.HasValue)
                {
                    var selected = available.FirstOrDefault(s => s.Id == _selectedSubscriptionId.Value);
                    if (selected != null)
                    {
                        Divider();
                        Navigation(
                            Button(l10n("user.subscriptions.actions.pay", new Dictionary<string, string> { ["price"] = selected.Price.ToString("C") })).OnClick(_ =>
                            {
                                try
                                {
                                    _subscriptionLogic.BuySubscription(_user.Id, selected.Id);
                                    _statusMessage = l10n("user.subscriptions.status.bought", new Dictionary<string, string> { ["name"] = selected.Name });
                                    _selectedSubscriptionId = null;
                                }
                                catch (InvalidOperationException ex)
                                {
                                    _statusMessage = ex.Message;
                                }
                                SetUser(_user);
                            })
                        );
                    }
                }
            }
        }

        Divider();
        Message(() => _statusMessage);
        Navigation(
            Button(l10n("main.dashboard.actions.back")).OnClick(() =>
            {
                Console.Clear();
                var mainView = _serviceProvider.GetRequiredService<MainView>();
                mainView.SetUser(_user);
                _appLoop.Display(mainView);
            })
        );
    }
}
