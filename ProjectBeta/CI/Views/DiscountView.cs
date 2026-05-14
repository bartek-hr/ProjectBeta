using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class DiscountView : Form
{
    private readonly DiscountAccess _discountAccess;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user = null!;
    private string? _statusMessage;
    private int? _selectedDiscountId;
    private bool _createMode;

    private static readonly string[] DayOptions =
        ["None", .. Enum.GetNames<DayOfWeek>()];

    private static string DayOfWeekToOption(int? day) =>
        day.HasValue ? ((DayOfWeek)day.Value).ToString() : "None";

    private static int? OptionToDayOfWeek(string? option) =>
        option == null || option == "None" ? null : (int)Enum.Parse<DayOfWeek>(option);

    public DiscountView(DiscountAccess discountAccess, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _discountAccess = discountAccess;
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
        Heading(l10n("admin.discounts.heading"));
        Divider();

        var discounts = _discountAccess.GetActive();

        if (!_createMode && !_selectedDiscountId.HasValue && discounts.Count > 0)
        {
            _selectedDiscountId = discounts[0].Id;
        }

        foreach (var d in discounts)
        {
            var discount = d;
            var isSelected = !_createMode && _selectedDiscountId == discount.Id;
            Button((isSelected ? "> " : "  ") + discount.Name).OnClick(() =>
            {
                _createMode = false;
                _selectedDiscountId = discount.Id;
                _statusMessage = null;
                SetUser(_user);
            });
        }

        Divider();

        if (_createMode)
        {
            RenderCreateForm();
        }
        else if (_selectedDiscountId.HasValue)
        {
            var selected = discounts.FirstOrDefault(d => d.Id == _selectedDiscountId.Value);
            if (selected != null)
                RenderDetailForm(selected);
        }

        Divider();

        Button((_createMode ? "> " : "  ") + l10n("admin.discounts.actions.create")).OnClick(() =>
        {
            _createMode = true;
            _selectedDiscountId = null;
            _statusMessage = null;
            SetUser(_user);
        });

        Message(() => _statusMessage);
        Button(l10n("admin.discounts.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }

    private void RenderDetailForm(Discount discount)
    {
        Label(discount.Name);

        var nameInput = TextInput(l10n("admin.discounts.fields.name.label"))
            .Default(discount.Name);

        var percentageInput = NumberInput(l10n("admin.discounts.fields.percentage.label"))
            .Min(0).Max(100).Step(1).Precision(0).Default((double)discount.Percentage);

        var minAgeInput = NumberInput(l10n("admin.discounts.fields.min_age.label"))
            .Min(0).Max(120).Step(1).Precision(0);
        if (discount.MinAge.HasValue) minAgeInput.Default(discount.MinAge.Value);

        var maxAgeInput = NumberInput(l10n("admin.discounts.fields.max_age.label"))
            .Min(0).Max(120).Step(1).Precision(0);
        if (discount.MaxAge.HasValue) maxAgeInput.Default(discount.MaxAge.Value);

        var minGroupSizeInput = NumberInput(l10n("admin.discounts.fields.min_group_size.label"))
            .Min(1).Step(1).Precision(0);
        if (discount.MinGroupSize.HasValue) minGroupSizeInput.Default(discount.MinGroupSize.Value);

        var effectiveFromInput = TextInput(l10n("admin.discounts.fields.effective_from.label"))
            .Default(discount.EffectiveFrom.ToString("yyyy-MM-dd"));

        var effectiveUntilInput = TextInput(l10n("admin.discounts.fields.effective_until.label"))
            .Default(discount.EffectiveUntil?.ToString("yyyy-MM-dd") ?? string.Empty);

        var dowGroup = RadioGroup(l10n("admin.discounts.fields.required_day_of_week.label"));
        foreach (var opt in DayOptions) dowGroup.AddOption(opt);
        dowGroup.Default(DayOfWeekToOption(discount.RequiredDayOfWeek));

        Button(l10n("admin.discounts.actions.save", new Dictionary<string, string> { ["name"] = discount.Name }))
            .OnClick(_ =>
            {
                if (string.IsNullOrWhiteSpace(nameInput.Value))
                { _statusMessage = l10n("admin.discounts.status.name_required"); return; }

                if (!percentageInput.Value.HasValue || percentageInput.Value.Value < 0 || percentageInput.Value.Value > 100)
                { _statusMessage = l10n("admin.discounts.status.invalid_percentage"); return; }

                if (minAgeInput.Value.HasValue && maxAgeInput.Value.HasValue && minAgeInput.Value.Value > maxAgeInput.Value.Value)
                { _statusMessage = l10n("admin.discounts.status.invalid_age_range"); return; }

                if (!TryParseDate(effectiveFromInput.Value, out var parsedFrom))
                { _statusMessage = l10n("admin.discounts.status.invalid_date"); return; }

                DateTime? parsedUntil = null;
                if (!string.IsNullOrWhiteSpace(effectiveUntilInput.Value))
                {
                    if (!TryParseDate(effectiveUntilInput.Value, out var until))
                    { _statusMessage = l10n("admin.discounts.status.invalid_date"); return; }
                    parsedUntil = until;
                }

                discount.Name = nameInput.Value!.Trim();
                discount.Percentage = (decimal)percentageInput.Value.Value;
                discount.MinAge = minAgeInput.Value.HasValue ? (int)minAgeInput.Value.Value : null;
                discount.MaxAge = maxAgeInput.Value.HasValue ? (int)maxAgeInput.Value.Value : null;
                discount.MinGroupSize = minGroupSizeInput.Value.HasValue ? (int)minGroupSizeInput.Value.Value : null;
                discount.EffectiveFrom = parsedFrom;
                discount.EffectiveUntil = parsedUntil;
                discount.RequiredDayOfWeek = OptionToDayOfWeek(dowGroup.Value);
                _discountAccess.Update(discount);
                _statusMessage = l10n("admin.discounts.status.saved", new Dictionary<string, string> { ["name"] = discount.Name });
                SetUser(_user);
            });

        Button(l10n("admin.discounts.actions.delete", new Dictionary<string, string> { ["name"] = discount.Name }))
            .OnClick(_ =>
            {
                var name = discount.Name;
                _discountAccess.Delete(discount.Id);
                _selectedDiscountId = null;
                _statusMessage = l10n("admin.discounts.status.deleted", new Dictionary<string, string> { ["name"] = name });
                SetUser(_user);
            });
    }

    private void RenderCreateForm()
    {
        Label(l10n("admin.discounts.create.heading"));

        var newNameInput = TextInput(l10n("admin.discounts.fields.name.label"));

        var newPercentageInput = NumberInput(l10n("admin.discounts.fields.percentage.label"))
            .Min(0).Max(100).Step(1).Precision(0);

        var newMinAgeInput = NumberInput(l10n("admin.discounts.fields.min_age.label"))
            .Min(0).Max(120).Step(1).Precision(0);

        var newMaxAgeInput = NumberInput(l10n("admin.discounts.fields.max_age.label"))
            .Min(0).Max(120).Step(1).Precision(0);

        var newMinGroupSizeInput = NumberInput(l10n("admin.discounts.fields.min_group_size.label"))
            .Min(1).Step(1).Precision(0);

        var newEffectiveFromInput = TextInput(l10n("admin.discounts.fields.effective_from.label"));
        var newEffectiveUntilInput = TextInput(l10n("admin.discounts.fields.effective_until.label"));

        var newDowGroup = RadioGroup(l10n("admin.discounts.fields.required_day_of_week.label"));
        foreach (var opt in DayOptions) newDowGroup.AddOption(opt);
        newDowGroup.Default("None");

        Button(l10n("admin.discounts.actions.create")).OnClick(_ =>
        {
            if (string.IsNullOrWhiteSpace(newNameInput.Value))
            { _statusMessage = l10n("admin.discounts.status.name_required"); return; }

            if (!newPercentageInput.Value.HasValue || newPercentageInput.Value.Value < 0 || newPercentageInput.Value.Value > 100)
            { _statusMessage = l10n("admin.discounts.status.invalid_percentage"); return; }

            if (newMinAgeInput.Value.HasValue && newMaxAgeInput.Value.HasValue && newMinAgeInput.Value.Value > newMaxAgeInput.Value.Value)
            { _statusMessage = l10n("admin.discounts.status.invalid_age_range"); return; }

            if (!TryParseDate(newEffectiveFromInput.Value, out var newFrom))
            { _statusMessage = l10n("admin.discounts.status.invalid_date"); return; }

            DateTime? newUntil = null;
            if (!string.IsNullOrWhiteSpace(newEffectiveUntilInput.Value))
            {
                if (!TryParseDate(newEffectiveUntilInput.Value, out var until))
                { _statusMessage = l10n("admin.discounts.status.invalid_date"); return; }
                newUntil = until;
            }

            var created = new Discount
            {
                Name = newNameInput.Value!.Trim(),
                Percentage = (decimal)newPercentageInput.Value.Value,
                MinAge = newMinAgeInput.Value.HasValue ? (int)newMinAgeInput.Value.Value : null,
                MaxAge = newMaxAgeInput.Value.HasValue ? (int)newMaxAgeInput.Value.Value : null,
                MinGroupSize = newMinGroupSizeInput.Value.HasValue ? (int)newMinGroupSizeInput.Value.Value : null,
                EffectiveFrom = newFrom,
                EffectiveUntil = newUntil,
                RequiredDayOfWeek = OptionToDayOfWeek(newDowGroup.Value),
                IsActive = true,
            };
            _discountAccess.Add(created);
            _selectedDiscountId = created.Id;
            _createMode = false;
            _statusMessage = l10n("admin.discounts.status.created", new Dictionary<string, string> { ["name"] = created.Name });
            SetUser(_user);
        });
    }

    private static bool TryParseDate(string? input, out DateTime result)
    {
        return DateTime.TryParseExact(
            input?.Trim(),
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out result);
    }
}
