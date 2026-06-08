using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Views;

public sealed class SubscriptionView : Form
{
    private readonly SubscriptionLogic _subscriptionLogic;
    private readonly SeatPriceAccess _seatPriceAccess;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user = null!;
    private string? _statusMessage;
    private int? _selectedSubscriptionId;
    private bool _createMode;

    private static readonly string[] DayOptions =
        ["None", .. Enum.GetNames<DayOfWeek>()];

    private static string DayOfWeekToOption(int? day) =>
        day.HasValue ? ((DayOfWeek)day.Value).ToString() : "None";

    private static int? OptionToDayOfWeek(string? option) =>
        option == null || option == "None" ? null : (int)Enum.Parse<DayOfWeek>(option);

    public SubscriptionView(SubscriptionLogic subscriptionLogic, SeatPriceAccess seatPriceAccess, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _subscriptionLogic = subscriptionLogic;
        _seatPriceAccess = seatPriceAccess;
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
        Heading(l10n("admin.subscriptions.heading"));
        Divider();

        // Show active subscription for this user (if any)
        var activeSub = _subscriptionLogic
            .GetAvailableSubscriptionsWithSeatPrice()
            .FirstOrDefault(s => s.UserSubscriptions != null && s.UserSubscriptions.Any(us => us.UserId == _user.Id && us.IsActive));
        if (activeSub != null)
        {
            Label(l10n("admin.subscriptions.menu.active_subscription"));
            Label($"{activeSub.Name} ({l10n("admin.subscriptions.fields.price.label")}: {activeSub.Price}, {l10n("admin.subscriptions.fields.seat_type.label")}: {activeSub.SeatPrice?.Name})");
            Divider();
        }

        var subs = _subscriptionLogic.GetAvailableSubscriptionsWithSeatPrice();
        if (!_createMode && !_selectedSubscriptionId.HasValue && subs.Count > 0)
        {
            _selectedSubscriptionId = subs[0].Id;
        }

        // List all subscriptions as selectable buttons
        var subButtons = new List<Button>();
        Button? selectedButton = null;
        if (subs.Count == 0)
        {
            Label(l10n("admin.subscriptions.menu.no_subscriptions"));
        }
        else
        {
            foreach (var s in subs)
            {
                var subscription = s;
                var isSelected = !_createMode && _selectedSubscriptionId == subscription.Id;
                var day = subscription.ApplicableDayOfWeek.HasValue ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)subscription.ApplicableDayOfWeek.Value) : l10n("components.form.none");
                var label = $"{(isSelected ? "> " : "  ")}{subscription.Name} ({l10n("admin.subscriptions.fields.price.label")}: {subscription.Price}, {l10n("admin.subscriptions.fields.applicable_day.label")}: {day}, {l10n("admin.subscriptions.fields.seat_type.label")}: {subscription.SeatPrice?.Name})";
                var button = Button(label).OnClick(() =>
                {
                    _createMode = false;
                    _selectedSubscriptionId = subscription.Id;
                    _statusMessage = null;
                    SetUser(_user);
                });
                subButtons.Add(button);
                if (isSelected)
                    selectedButton = button;
            }
            var navigation = Navigation(subButtons.ToArray());
            if (selectedButton != null)
                navigation.SetActive(selectedButton);
        }

        Divider();

        // Show selected subscription details or create form
        if (_createMode)
        {
            RenderCreateForm();
        }
        else if (_selectedSubscriptionId.HasValue)
        {
            var selected = subs.FirstOrDefault(s => s.Id == _selectedSubscriptionId.Value);
            if (selected != null)
                RenderDetailForm(selected);
        }

        Divider();

        Message(() => _statusMessage);
        var backButton = Button(l10n("main.dashboard.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
        if (_createMode)
        {
            Navigation(backButton);
        }
        else
        {
            var createButton = Button("  " + l10n("admin.subscriptions.actions.add")).OnClick(() =>
            {
                _createMode = true;
                _selectedSubscriptionId = null;
                _statusMessage = null;
                SetUser(_user);
            });
            Navigation(createButton, backButton);
        }
    }

    private void RenderCreateForm()
    {
        Label(l10n("admin.subscriptions.actions.add"));

        var nameInput = TextInput(l10n("admin.subscriptions.fields.name.label")).Key("name");
        var priceInput = NumberInput(l10n("admin.subscriptions.fields.price.label")).Min(0).Step(0.01).Precision(2).Default(0);
        Label(l10n("admin.subscriptions.fields.seat_type.hint"), Style.Warning);
        var seatTypeGroup = RadioGroup<int>(l10n("admin.subscriptions.fields.seat_type.label")).Required();
        var seatTypes = _seatPriceAccess.GetAll();
        foreach (var seatType in seatTypes)
        {
            seatTypeGroup.AddOption(seatType.Id, seatType.Name);
        }
        if (seatTypes.Count > 0)
        {
            seatTypeGroup.Default(seatTypes[0].Id);
        }
        Label(l10n("admin.subscriptions.fields.connect_discount.hint"), Style.Warning);
        var sharedAllowedInput = Checkbox(l10n("admin.subscriptions.fields.is_connect_allowed.label"));
        var sharedDiscountInput = NumberInput(l10n("admin.subscriptions.fields.connect_discount.label")).Min(0).Max(100).Step(1).Precision(0).Default(0);
        var effectiveFromInput = TextInput(l10n("admin.subscriptions.fields.effective_from.label")).Key("effective_from").Default(DateTime.UtcNow.ToString("yyyy-MM-dd"));
        var effectiveUntilInput = TextInput(l10n("admin.subscriptions.fields.effective_until.label")).Key("effective_until");

        var dayOfWeekGroup = RadioGroup(l10n("admin.subscriptions.fields.applicable_day.label"));
        foreach (var opt in DayOptions) dayOfWeekGroup.AddOption(opt);
        dayOfWeekGroup.Default(DayOptions[0]);

        Navigation(
            Button(l10n("admin.subscriptions.actions.add")).OnClick(_ =>
            {
                if (string.IsNullOrWhiteSpace(nameInput.Value))
                { _statusMessage = l10n("admin.subscriptions.status.name_required"); return; }

                DateTime effectiveFrom = DateTime.UtcNow;
                DateTime? effectiveUntil = null;
                if (!string.IsNullOrWhiteSpace(effectiveFromInput.Value) && !DateTime.TryParse(effectiveFromInput.Value, out effectiveFrom))
                {
                    _statusMessage = l10n("subscriptions.status.invalid_effective_from");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(effectiveUntilInput.Value))
                {
                    if (DateTime.TryParse(effectiveUntilInput.Value, out var until))
                        effectiveUntil = until;
                    else
                    {
                        _statusMessage = l10n("subscriptions.status.invalid_effective_until");
                        return;
                    }
                }

                var sub = new Subscription
                {
                    Name = nameInput.Value!.Trim(),
                    Price = (decimal)(priceInput.Value ?? 0),
                    ApplicableDayOfWeek = OptionToDayOfWeek(dayOfWeekGroup.Value),
                    SeatPriceId = seatTypeGroup.HasValue ? seatTypeGroup.Value : seatTypes.FirstOrDefault()?.Id ?? 1,
                    IsConnectAllowed = sharedAllowedInput.Value,
                    ConnectDiscount = (decimal)(sharedDiscountInput.Value ?? 0),
                    IsActive = true,
                    EffectiveFrom = effectiveFrom,
                    EffectiveUntil = effectiveUntil
                };
                _subscriptionLogic.AddSubscription(sub);
                _statusMessage = l10n("admin.subscriptions.status.added");
                SetUser(_user);
            })
        );
    }

    private void RenderDetailForm(Subscription s)
    {
        Label(s.Name, Style.Warning);

        var nameInput = TextInput(l10n("admin.subscriptions.fields.name.label")).Key("name").Default(s.Name);
        var priceInput = NumberInput(l10n("admin.subscriptions.fields.price.label")).Min(0).Step(0.01).Precision(2).Default((double)s.Price);
        Label(l10n("admin.subscriptions.fields.seat_type.hint"), Style.Warning);
        var seatTypeGroup = RadioGroup<int>(l10n("admin.subscriptions.fields.seat_type.label")).Required();
        var seatTypes = _seatPriceAccess.GetAll();
        foreach (var seatType in seatTypes)
        {
            seatTypeGroup.AddOption(seatType.Id, seatType.Name);
        }
        seatTypeGroup.Default(s.SeatPriceId);
        Label(l10n("admin.subscriptions.fields.connect_discount.hint"), Style.Warning);
        var sharedAllowedInput = Checkbox(l10n("admin.subscriptions.fields.is_connect_allowed.label")).Default(s.IsConnectAllowed);
        var sharedDiscountInput = NumberInput(l10n("admin.subscriptions.fields.connect_discount.label")).Min(0).Max(100).Step(1).Precision(0).Default((double)s.ConnectDiscount);
        var effectiveFromInput = TextInput(l10n("admin.subscriptions.fields.effective_from.label")).Key("effective_from").Default(s.EffectiveFrom.ToString("yyyy-MM-dd"));
        var effectiveUntilInput = TextInput(l10n("admin.subscriptions.fields.effective_until.label")).Key("effective_until").Default(s.EffectiveUntil?.ToString("yyyy-MM-dd") ?? "");
        var dayOfWeek = RadioGroup(l10n("admin.subscriptions.fields.applicable_day.label"));
        foreach (var opt in DayOptions)
        {
            dayOfWeek.AddOption(opt);
        }
        dayOfWeek.Default(DayOfWeekToOption(s.ApplicableDayOfWeek));

        Navigation(
            Button(l10n("admin.subscriptions.actions.save", new Dictionary<string, string> { ["name"] = s.Name })).OnClick(_ =>
            {
                if (string.IsNullOrWhiteSpace(nameInput.Value))
                { _statusMessage = l10n("admin.subscriptions.status.name_required"); return; }

                DateTime effectiveFrom = s.EffectiveFrom;
                DateTime? effectiveUntil = s.EffectiveUntil;
                if (!string.IsNullOrWhiteSpace(effectiveFromInput.Value) && !DateTime.TryParse(effectiveFromInput.Value, out effectiveFrom))
                {
                    _statusMessage = l10n("subscriptions.status.invalid_date");
                    return;
                }
                if (!string.IsNullOrWhiteSpace(effectiveUntilInput.Value))
                {
                    if (DateTime.TryParse(effectiveUntilInput.Value, out var until))
                        effectiveUntil = until;
                    else
                    {
                        _statusMessage = l10n("subscriptions.status.invalid_date");
                        return;
                    }
                }

                s.Name = nameInput.Value!.Trim();
                s.Price = (decimal)(priceInput.Value ?? 0);
                s.ApplicableDayOfWeek = OptionToDayOfWeek(dayOfWeek.Value);
                s.SeatPriceId = seatTypeGroup.HasValue ? seatTypeGroup.Value : s.SeatPriceId;
                s.IsConnectAllowed = sharedAllowedInput.Value;
                s.ConnectDiscount = (decimal)(sharedDiscountInput.Value ?? 0);
                s.EffectiveFrom = effectiveFrom;
                s.EffectiveUntil = effectiveUntil;
                _subscriptionLogic.AddSubscription(s);
                _statusMessage = l10n("admin.subscriptions.status.saved");
                SetUser(_user);
            }),
            Button(l10n("admin.subscriptions.actions.remove")).OnClick(_ =>
            {
                _subscriptionLogic.RemoveSubscription(s.Id);
                _statusMessage = l10n("admin.subscriptions.status.removed");
                SetUser(_user);
            })
        );
    }
}
