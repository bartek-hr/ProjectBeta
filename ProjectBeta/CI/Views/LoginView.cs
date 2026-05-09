using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Views;
using ProjectBeta.Logic;
using ProjectBeta.Model;

public sealed class LoginView : Form
{
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public LoginView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _appLoop = appLoop;
        InitializeForm();
    }

    private void InitializeForm(bool invalidCredentials = false)
    {
        ClearChildren();

        Heading(l10n("auth.login.heading"));
        Label(invalidCredentials
            ? l10n("auth.login.instructions.invalid")
            : l10n("auth.login.instructions.default"));
        Divider();

        Message(() => GetError("general"));

        TextInput(l10n("auth.login.fields.identity.label"))
            .Key("identity")
            .Placeholder(l10n("auth.login.fields.identity.placeholder"))
            .Required();
        Message(() => GetError("identity"));

        TextInput(l10n("auth.login.fields.password.label"))
            .Key("password")
            .Placeholder(l10n("auth.login.fields.password.placeholder"))
            .Required()
            .Masked();
        Message(() => GetError("password"));

        Divider();

        Message(() => _statusMessage);
        Button(l10n("auth.login.actions.submit")).OnClick(OnSubmit);
        Button(l10n("auth.login.actions.register")).OnClick(NavigateToUserView);
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

        var identity = form.Get<string>("identity");
        var password = form.Get<string>("password");

        var result = _userLogic.SearchUser(identity, identity, password);

        if (!result.Success)
        {
            _fieldErrors = result.FieldErrors;
            _statusMessage = l10n("auth.login.status.failed");
            InitializeForm(result.FieldErrors?.ContainsKey("identity") == true);
            return;
        }

        _statusMessage = l10n("auth.login.status.success", new Dictionary<string, string>
        {
            ["name"] = result.User!.Username
        });
        _fieldErrors = null;
        NavigateToMainView(result.User);
    }

    private void NavigateToMainView(User user)
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(user);
        _appLoop.Display(mainView);
    }

    private void NavigateToUserView()
    {
        Console.Clear();
        _appLoop.Display(_serviceProvider.GetRequiredService<UserView>());
    }
}
