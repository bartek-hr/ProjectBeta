using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class AccountView : Form
{
    private readonly UserLogic _userLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private User? _adminUser;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Navigation? _confirmDeleteNavigation;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public AccountView(UserLogic userLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _userLogic = userLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetUser(User user, User? adminUser = null)
    {
        _user = user;
        _adminUser = adminUser;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm(AccountFormState? state = null)
    {
        ClearChildren();

        Heading(l10n("account.profile.heading"));
        LanguageToggle(SwitchLanguage, () => _confirmingDelete);
        Label(l10n("account.profile.instructions"));
        Divider();

        Message(() => GetError("general"));

        Label(l10n("account.profile.sections.account"));
        var canEditRole = _adminUser?.IsSuperAdmin() == true && !_user.IsSuperAdmin();

        TextInput(l10n("account.profile.fields.username.label"))
            .Key("username")
            .Placeholder(l10n("account.profile.fields.username.placeholder"))
            .Required()
            .Min(3)
            .Max(20)
            .Default(state?.Username ?? _user.Username);
        Message(() => GetError("username"));

        TextInput(l10n("account.profile.fields.email.label"))
            .Key("email")
            .Placeholder(l10n("account.profile.fields.email.placeholder"))
            .Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", l10n("validation.user.email.invalid"))
            .Default(state?.Email ?? _user.Email);
        Message(() => GetError("email"));

        TextInput(l10n("account.profile.fields.new_password.label"))
            .Key("new_password")
            .Placeholder(l10n("account.profile.fields.new_password.placeholder"))
            .Masked()
            .Default(state?.NewPassword ?? string.Empty);
        Message(() => GetError("new_password"));

        TextInput(l10n("account.profile.fields.first_name.label"))
            .Key("first_name")
            .Placeholder(l10n("account.profile.fields.first_name.placeholder"))
            .Required()
            .Default(state?.FirstName ?? _user.FirstName);
        Message(() => GetError("first_name"));

        TextInput(l10n("account.profile.fields.last_name.label"))
            .Key("last_name")
            .Placeholder(l10n("account.profile.fields.last_name.placeholder"))
            .Required()
            .Default(state?.LastName ?? _user.LastName);
        Message(() => GetError("last_name"));

        DateInput(l10n("account.profile.fields.date_of_birth.label"))
            .Key("date_of_birth")
            .Required()
            .Min(new DateOnly(1900, 1, 1))
            .Max(DateOnly.FromDateTime(DateTime.Today))
            .Default(state?.DateOfBirth ?? _user.DateOfBirth);
        Message(() => GetError("date_of_birth"));

        if (canEditRole)
        {
            RadioGroup(l10n("account.profile.fields.role.label"))
                .Key("role")
                .AddOption(l10n("roles.user"))
                .AddOption(l10n("roles.admin"))
                .Default(GetRoleOptionLabel(state?.Role ?? _user.Role))
                .Required();
            Message(() => GetError("role"));
        }

        Divider();

        Message(() => _statusMessage);
        var saveButton = new Button(l10n("account.profile.actions.save")).OnClick(OnSave);
        saveButton.Hidden(() => _confirmingDelete);

        var deleteButton = new Button(l10n("account.profile.actions.delete_account")).OnClick(() =>
        {
            _confirmingDelete = true;
            if (_confirmDeleteNavigation != null && _noCancelButton != null)
            {
                _confirmDeleteNavigation.SetActive(_noCancelButton);
                FocusChild(_confirmDeleteNavigation);
            }

            Invalidate();
            Render();
        });
        deleteButton.Hidden(() => _confirmingDelete);

        var backButton = new Button(l10n("account.profile.actions.back")).OnClick(NavigateToMain);
        backButton.Hidden(() => _confirmingDelete);
        Navigation(saveButton, deleteButton, backButton);

        Message(() => _confirmingDelete ? l10n("account.profile.confirm_delete.message") : null);
        var confirmDeleteButton = new Button(l10n("account.profile.confirm_delete.confirm")).OnClick(OnDelete);
        confirmDeleteButton.Hidden(() => !_confirmingDelete);
        _noCancelButton = new Button(l10n("account.profile.confirm_delete.cancel")).OnClick(() =>
        {
            _confirmingDelete = false;
            Invalidate();
            Render();
        });
        _noCancelButton.Hidden(() => !_confirmingDelete);
        _confirmDeleteNavigation = Navigation(confirmDeleteButton, _noCancelButton);
    }

    private string? GetError(string key)
    {
        return _fieldErrors != null && _fieldErrors.ContainsKey(key)
            ? string.Join("\n", _fieldErrors[key])
            : null;
    }

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var username = form.Get<string>("username");
        var email = form.Get<string>("email");
        var newPassword = form.Get<string>("new_password");
        var firstName = form.Get<string>("first_name");
        var lastName = form.Get<string>("last_name");
        var dateOfBirth = form.Get<DateOnly?>("date_of_birth");
        var requestedRole = ResolveSelectedRole(form.Get<string>("role"));
        var actingUser = _adminUser ?? _user;

        var result = _userLogic.UpdateUser(
            _user.Id,
            username!,
            email!,
            newPassword,
            firstName!,
            lastName!,
            dateOfBirth,
            actingUser,
            requestedRole
        );

        if (!result.Success)
        {
            _fieldErrors = result.FieldErrors;
            return;
        }

        _user.Username = username!;
        _user.Email = email!;
        _user.FirstName = firstName!;
        _user.LastName = lastName!;
        _user.DateOfBirth = dateOfBirth!.Value;
        if (!string.IsNullOrWhiteSpace(requestedRole) && !_user.IsSuperAdmin())
            _user.Role = requestedRole;

        _statusMessage = l10n("account.profile.status.updated");
        _fieldErrors = null;
        Invalidate();
        Render();
        Thread.Sleep(1200);
    }

    private void OnDelete(Form form)
    {
        var result = _userLogic.DeleteUser(_user.Id);

        if (!result.Success)
        {
            _confirmingDelete = false;
            _fieldErrors = result.FieldErrors;
            return;
        }

        _statusMessage = l10n("account.profile.status.deleted");
        _fieldErrors = null;
        Invalidate();
        Render();
        Thread.Sleep(1500);
        Console.Clear();

        if (_adminUser != null)
        {
            var usersView = _serviceProvider.GetRequiredService<UsersView>();
            usersView.SetUser(_adminUser);
            _appLoop.Display(usersView);
            return;
        }

        _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_adminUser ?? _user);
        _appLoop.Display(mainView);
    }

    private void SwitchLanguage()
    {
        var targetLocale = GetLocale().Equals("en-GB", StringComparison.OrdinalIgnoreCase) ? "nl-NL" : "en-GB";
        var state = CaptureState(this);
        _statusMessage = null;
        _fieldErrors = null;
        _confirmingDelete = false;
        SetLocale(targetLocale);
        InitializeForm(state);
        Invalidate();
        Render();
    }

    private static AccountFormState CaptureState(Form form)
    {
        return new AccountFormState(
            form.Get<string>("username"),
            form.Get<string>("email"),
            form.Get<string>("new_password"),
            form.Get<string>("first_name"),
            form.Get<string>("last_name"),
            form.Get<DateOnly?>("date_of_birth"),
            ResolveSelectedRole(form.Get<string>("role")));
    }

    private static string GetRoleOptionLabel(string role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? l10n("roles.admin")
            : l10n("roles.user");
    }

    private static string? ResolveSelectedRole(string? selectedRole)
    {
        if (string.Equals(selectedRole, l10n("roles.admin"), StringComparison.Ordinal))
            return "Admin";

        if (string.Equals(selectedRole, l10n("roles.user"), StringComparison.Ordinal))
            return "User";

        return null;
    }

    private sealed record AccountFormState(
        string? Username,
        string? Email,
        string? NewPassword,
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth,
        string? Role);
}
