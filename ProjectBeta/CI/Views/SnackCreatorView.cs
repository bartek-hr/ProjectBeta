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
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public SnackCreatorView(SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }
    public void SetView(User user)
    {
        _user = user;
        InitializeForm();
    }
    private void InitializeForm(UserFormState? state = null)
    {
        ClearChildren();

        Heading(l10n("Snack Creator"));
        LanguageToggle(SwitchLanguage);
        Label(l10n("auth.register.instructions"));
        Divider();

        Message(() => GetError("general"));

        var nameInput = TextInput(l10n("Name"))
            .Key("name")
            .Required()
            .Min(3)
            .Max(50);
        if (!string.IsNullOrEmpty(state?.Name))
            nameInput.Default(state.Name);
        Message(() => GetError("name"));

        var cinemaInput = TextInput(l10n("CinemaId"))
            .Key("cinemaid")
            .Required();
        if (state?.CinemaId > 0)
            cinemaInput.Default(state.CinemaId.ToString());
        Message(() => GetError("cinemaid"));

        var priceInput = TextInput(l10n("Price"))
            .Key("price")
            .Required();
        if (state?.Price > 0)
            priceInput.Default(state.Price.ToString());
        Message(() => GetError("price"));

        var quantityInput = TextInput(l10n("Quantity"))
            .Key("quantity")
            .Required();
        if (state?.Quantity > 0)
            quantityInput.Default(state.Quantity.ToString());
        Message(() => GetError("quantity"));

        Divider();

        Message(() => _statusMessage);
        Navigation(
            Button(l10n("auth.register.actions.submit")).OnClick(OnSubmit),
            Button(l10n("auth.register.actions.cancel")).OnClick(() =>
            {
                Console.Clear();
                _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
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
        Snack SnackToAdd = new Snack{
            Name = form.Get<string>("name")!,
            CinemaId = int.Parse(form.Get<string>("cinemaid")!),
            Price = decimal.Parse(form.Get<string>("price")!),
            Quantity = int.Parse(form.Get<string>("quantity")!)
        };
        _snackLogic.Add(SnackToAdd, _user);
        _statusMessage = l10n("auth.register.status.success");
        _fieldErrors = null;
        Invalidate();
        Render();
        Thread.Sleep(1500);
        Console.Clear();
        NavigateToMain();
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
    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
    private static UserFormState CaptureState(Form form)
    {
        return new UserFormState(
            form.Get<string>("name"),
            form.Get<int>("cinemaid"),
            form.Get<decimal>("price"),
            form.Get<int>("quantity"));
    }

    private sealed record UserFormState(
        string? Name,
        int? CinemaId,
        decimal? Price,
        int? Quantity);
}
