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

    private void InitializeForm(UserFormState? state = null)
    {
        ClearChildren();

        Heading(l10n("auth.register.heading"));
        LanguageToggle(SwitchLanguage);
        Label(l10n("auth.register.instructions"));
        Divider();

        Message(() => GetError("general"));

        Label(l10n("auth.register.sections.account"));
        var usernameInput = TextInput(l10n("auth.register.fields.username.label"))
            .Key("username")
            .Placeholder(l10n("auth.register.fields.username.placeholder"))
            .Required()
            .Min(3)
            .Max(20);
        if (!string.IsNullOrEmpty(state?.Username))
            usernameInput.Default(state.Username);
        Message(() => GetError("username"));

        var emailInput = TextInput(l10n("auth.register.fields.email.label"))
            .Key("email")
            .Placeholder(l10n("auth.register.fields.email.placeholder"))
            .Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", l10n("validation.user.email.invalid"));
        if (!string.IsNullOrEmpty(state?.Email))
            emailInput.Default(state.Email);
        Message(() => GetError("email"));

        var passwordInput = TextInput(l10n("auth.register.fields.password.label"))
            .Key("password")
            .Placeholder(l10n("auth.register.fields.password.placeholder"))
            .Required()
            .Masked();
        if (!string.IsNullOrEmpty(state?.Password))
            passwordInput.Default(state.Password);
        Message(() => GetError("password"));

        var firstNameInput = TextInput(l10n("auth.register.fields.first_name.label"))
            .Key("first_name")
            .Placeholder(l10n("auth.register.fields.first_name.placeholder"))
            .Required();
        if (!string.IsNullOrEmpty(state?.FirstName))
            firstNameInput.Default(state.FirstName);
        Message(() => GetError("first_name"));

        var lastNameInput = TextInput(l10n("auth.register.fields.last_name.label"))
            .Key("last_name")
            .Placeholder(l10n("auth.register.fields.last_name.placeholder"))
            .Required();
        if (!string.IsNullOrEmpty(state?.LastName))
            lastNameInput.Default(state.LastName);
        Message(() => GetError("last_name"));

        var dateOfBirthInput = DateInput(l10n("auth.register.fields.date_of_birth.label"))
            .Key("date_of_birth")
            .Required()
            .Min(new DateOnly(1900, 1, 1))
            .Max(DateOnly.FromDateTime(DateTime.Today));
        if (state?.DateOfBirth is DateOnly dateOfBirth)
            dateOfBirthInput.Default(dateOfBirth);
        Message(() => GetError("date_of_birth"));

        Divider();

        Message(() => _statusMessage);
        Navigation(
            Button(l10n("auth.register.actions.submit")).OnClick(OnSubmit),
            Button(l10n("auth.register.actions.reset")).OnClick(OnReset),
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

    private static UserFormState CaptureState(Form form)
    {
        return new UserFormState(
            form.Get<string>("username"),
            form.Get<string>("email"),
            form.Get<string>("password"),
            form.Get<string>("first_name"),
            form.Get<string>("last_name"),
            form.Get<DateOnly?>("date_of_birth"));
    }

    private sealed record UserFormState(
        string? Username,
        string? Email,
        string? Password,
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth);
}
