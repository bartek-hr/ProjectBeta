using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Rendering;
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
    private string? _connectEmail;

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

            Label(l10n("user.subscriptions.current_label"), Style.Default);
            Label($"  {activeSub.Name} — {l10n("user.subscriptions.price_label")}: {activeSub.Price:C}, {l10n("user.subscriptions.day_label")}: {day}, {l10n("user.subscriptions.seat_label")}: {activeSub.SeatPrice?.Name}", Style.Default);

            if (myUserSub.IsConnected && myUserSub.ConnectedWithUserId.HasValue)
            {
                var connectedEmail = _subscriptionLogic.GetUserEmail(myUserSub.ConnectedWithUserId.Value);
                var discountedPrice = activeSub.Price * (1 - (activeSub.ConnectDiscount / 100m));
                Label($"  {l10n("user.subscriptions.connected_with")}: {connectedEmail}", Style.Default);
                Label($"  {l10n("user.subscriptions.connected_discount")}: {activeSub.ConnectDiscount:0}% ({activeSub.Price:C} → {discountedPrice:C})", Style.Default);
            }
            else if (activeSub.IsConnectAllowed)
            {
                Divider();
                Label(l10n("user.subscriptions.connect.heading"), Style.Warning);
                Label(l10n("user.subscriptions.connect.instructions"), Style.Warning);
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
            Label(l10n("user.subscriptions.no_active"), Style.Warning);
            Divider();

            var available = allSubs;
            if (available.Count == 0)
            {
                Label(l10n("user.subscriptions.no_available"), Style.Warning);
            }
            else
            {
                Label(l10n("user.subscriptions.available_label"), Style.Default);

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

                        var friendCheck = !string.IsNullOrWhiteSpace(_connectEmail) && selected.IsConnectAllowed && selected.ConnectDiscount > 0
                            ? _subscriptionLogic.CheckFriendSubscription(selected.Id, _connectEmail)
                            : SubscriptionLogic.FriendCheckResult.NotFound;
                        bool discountApplied = friendCheck == SubscriptionLogic.FriendCheckResult.HasSubscription;

                        if (selected.IsConnectAllowed && selected.ConnectDiscount > 0)
                        {
                            var discountPct = (int)selected.ConnectDiscount;
                            Label(l10n("user.subscriptions.connect.discount_info",
                                new Dictionary<string, string> { ["discount"] = discountPct.ToString() }), Style.Primary);
                            var connectEmailInput = TextInput(l10n("user.subscriptions.connect.email_label"))
                                .Key("connect_email")
                                .Default(_connectEmail ?? "");
                            Navigation(
                                Button(l10n("user.subscriptions.connect.apply_discount")).OnClick(_ =>
                                {
                                    _connectEmail = connectEmailInput.Value?.Trim();
                                    _statusMessage = null;
                                    SetUser(_user);
                                })
                            );
                            if (discountApplied)
                                Label(l10n("user.subscriptions.connect.discount_applied",
                                    new Dictionary<string, string> { ["discount"] = discountPct.ToString() }), Style.Warning);
                            else if (friendCheck == SubscriptionLogic.FriendCheckResult.NotFound && !string.IsNullOrWhiteSpace(_connectEmail))
                                Label(l10n("user.subscriptions.errors.user_not_found"), Style.Error);
                            else if (friendCheck == SubscriptionLogic.FriendCheckResult.NoSubscription)
                                Label(l10n("user.subscriptions.errors.friend_no_subscription"), Style.Error);
                        }

                        var effectivePrice = discountApplied
                            ? selected.Price * (1 - (selected.ConnectDiscount / 100m))
                            : selected.Price;

                        Navigation(
                            Button(l10n("user.subscriptions.actions.pay", new Dictionary<string, string> { ["price"] = effectivePrice.ToString("C") })).OnClick(_ =>
                            {
                                try
                                {
                                    _subscriptionLogic.BuySubscription(_user.Id, selected.Id);
                                    if (discountApplied)
                                    {
                                        try { _subscriptionLogic.ConnectSubscription(_user.Id, _connectEmail!); }
                                        catch { /* connect is best-effort; subscription purchase already succeeded */ }
                                    }
                                    _statusMessage = l10n("user.subscriptions.status.bought", new Dictionary<string, string> { ["name"] = selected.Name });
                                    _selectedSubscriptionId = null;
                                    _connectEmail = null;
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
