using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;
namespace ProjectBeta.CI.Views;

public sealed class SnackCreatorView : Form
{
    private readonly SnackLogic _snackLogic;
    private readonly AppLoop _appLoop;
    private User _user;
    private int _locationId;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public SnackCreatorView(SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }
    public void SetView(User user, int locationId)
    {
        _user = user;
        _locationId = locationId;
        InitializeForm();
    }
    private void InitializeForm(UserFormState? state = null)
    {
        ClearChildren();

        Heading(l10n("snacks.creator.heading"));
        Divider();

        Message(() => GetError("general"));

        var nameInput = TextInput(l10n("snacks.creator.fields.name.label"))
            .Key("name")
            .Required()
            .Min(3)
            .Max(50);
        if (!string.IsNullOrEmpty(state?.Name))
            nameInput.Default(state.Name);
        Message(() => GetError("name"));

        var priceInput = TextInput(l10n("snacks.creator.fields.price.label"))
            .Key("price")
            .Required();
        if (state?.Price > 0)
            priceInput.Default(state.Price.ToString());
        Message(() => GetError("price"));

        var quantityInput = TextInput(l10n("snacks.creator.fields.quantity.label"))
            .Key("quantity")
            .Required();
        if (state?.Quantity > 0)
            quantityInput.Default(state.Quantity.ToString());
        Message(() => GetError("quantity"));

        Divider();

        Message(() => _statusMessage);
        Navigation(
            Button(l10n("snacks.creator.actions.save")).OnClick(OnSubmit),
            Button(l10n("snacks.creator.actions.cancel")).OnClick(() =>
            {
                Console.Clear();
                var snacksView = _serviceProvider.GetRequiredService<SnacksView>();
                snacksView.SetView(_user, _locationId);
                _appLoop.Display(snacksView);
            }));
    }

    private string? GetError(string key)
    {
        return _fieldErrors != null && _fieldErrors.ContainsKey(key)
            ? string.Join("\n", _fieldErrors[key])
            : null;
    }

    private void OnSubmit(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;
        Snack snackToAdd = new Snack
        {
            Name = form.Get<string>("name")!,
            LocationId = _locationId,
            Price = decimal.Parse(form.Get<string>("price")!),
            Quantity = int.Parse(form.Get<string>("quantity")!)
        };
        _snackLogic.Add(snackToAdd, _user);
        _statusMessage = l10n("snacks.creator.status.success");
        _fieldErrors = null;
        Invalidate();
        Render();
        Thread.Sleep(1500);
        Console.Clear();
        NavigateToSnacks();
    }

    private void SwitchLanguage()
    {
        var targetLocale = GetLocale().Equals("en-GB", StringComparison.OrdinalIgnoreCase) ? "nl-NL" : "en-GB";
        var state = CaptureState(this);
        _statusMessage = null;
        _fieldErrors = null;
        SetLocale(targetLocale);
        InitializeForm(state);
        Invalidate();
        Render();
    }

    private void NavigateToSnacks()
    {
        Console.Clear();
        var snacksView = _serviceProvider.GetRequiredService<SnacksView>();
        snacksView.SetView(_user, _locationId);
        _appLoop.Display(snacksView);
    }

    private static UserFormState CaptureState(Form form)
    {
        return new UserFormState(
            form.Get<string>("name"),
            form.Get<decimal>("price"),
            form.Get<int>("quantity"));
    }

    private sealed record UserFormState(
        string? Name,
        decimal? Price,
        int? Quantity);
}
