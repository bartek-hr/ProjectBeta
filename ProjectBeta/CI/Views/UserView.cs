using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;

public sealed class UserView : Form
{
    private readonly UserLogic _userLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public UserView(UserLogic userLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _userLogic = userLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("auth.register.heading"));
        Label(l10n("auth.register.instructions"));
        Divider();

        Message(() => GetError("general"));

        Label(l10n("auth.register.sections.account"));
        TextInput(l10n("auth.register.fields.username.label"))
            .Key("username")
            .Placeholder(l10n("auth.register.fields.username.placeholder"))
            .Required()
            .Min(3)
            .Max(20);
        Message(() => GetError("username"));

        TextInput(l10n("auth.register.fields.email.label"))
            .Key("email")
            .Placeholder(l10n("auth.register.fields.email.placeholder"))
            .Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", l10n("validation.user.email.invalid"));
        Message(() => GetError("email"));

        TextInput(l10n("auth.register.fields.password.label"))
            .Key("password")
            .Placeholder(l10n("auth.register.fields.password.placeholder"))
            .Required()
            .Masked();
        Message(() => GetError("password"));

        TextInput(l10n("auth.register.fields.first_name.label"))
            .Key("first_name")
            .Placeholder(l10n("auth.register.fields.first_name.placeholder"))
            .Required();
        Message(() => GetError("first_name"));

        TextInput(l10n("auth.register.fields.last_name.label"))
            .Key("last_name")
            .Placeholder(l10n("auth.register.fields.last_name.placeholder"))
            .Required();
        Message(() => GetError("last_name"));

        DateInput(l10n("auth.register.fields.date_of_birth.label"))
            .Key("date_of_birth")
            .Required()
            .Min(new DateOnly(1900, 1, 1))
            .Max(DateOnly.FromDateTime(DateTime.Today));
        Message(() => GetError("date_of_birth"));

        Divider();

        Message(() => _statusMessage);
        Button(l10n("auth.register.actions.submit")).OnClick(OnSubmit);
        Button(l10n("auth.register.actions.reset")).OnClick(OnReset);
        Button(l10n("auth.register.actions.cancel")).OnClick(() =>
        {
            Console.Clear();
            _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
        });
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

        var result = _userLogic.Register(
            form.Get<string>("username")!,
            form.Get<string>("email")!,
            form.Get<string>("password")!,
            form.Get<string>("first_name")!,
            form.Get<string>("last_name")!,
            form.Get<DateOnly?>("date_of_birth")
        );

        if (!result.Success)
        {
            _fieldErrors = result.FieldErrors;
            return;
        }

        _statusMessage = l10n("auth.register.status.success");
        _fieldErrors = null;
        Invalidate();
        Render();
        Thread.Sleep(1500);
        Console.Clear();
        _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
    }

    private void OnReset()
    {
        Console.Clear();
        _appLoop.Display(_serviceProvider.GetRequiredService<UserView>());
    }
}
