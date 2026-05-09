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

    private void InitializeForm()
    {
        Heading(l10n("account.profile.heading"));
        Label(l10n("account.profile.instructions"));
        Divider();

        Message(() => GetError("general"));

        Label(l10n("account.profile.sections.account"));
        TextInput(l10n("account.profile.fields.username.label"))
            .Key("username")
            .Placeholder(l10n("account.profile.fields.username.placeholder"))
            .Required()
            .Min(3)
            .Max(20)
            .Default(_user.Username);
        Message(() => GetError("username"));

        TextInput(l10n("account.profile.fields.email.label"))
            .Key("email")
            .Placeholder(l10n("account.profile.fields.email.placeholder"))
            .Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", l10n("validation.user.email.invalid"))
            .Default(_user.Email);
        Message(() => GetError("email"));

        TextInput(l10n("account.profile.fields.new_password.label"))
            .Key("new_password")
            .Placeholder(l10n("account.profile.fields.new_password.placeholder"))
            .Masked();
        Message(() => GetError("new_password"));

        TextInput(l10n("account.profile.fields.first_name.label"))
            .Key("first_name")
            .Placeholder(l10n("account.profile.fields.first_name.placeholder"))
            .Required()
            .Default(_user.FirstName);
        Message(() => GetError("first_name"));

        TextInput(l10n("account.profile.fields.last_name.label"))
            .Key("last_name")
            .Placeholder(l10n("account.profile.fields.last_name.placeholder"))
            .Required()
            .Default(_user.LastName);
        Message(() => GetError("last_name"));

        DateInput(l10n("account.profile.fields.date_of_birth.label"))
            .Key("date_of_birth")
            .Required()
            .Min(new DateOnly(1900, 1, 1))
            .Max(DateOnly.FromDateTime(DateTime.Today))
            .Default(_user.DateOfBirth);
        Message(() => GetError("date_of_birth"));

        Divider();

        Message(() => _statusMessage);
        Button(l10n("account.profile.actions.save")).OnClick(OnSave);
        Button(l10n("account.profile.actions.delete_account")).OnClick(() =>
        {
            _confirmingDelete = true;
            Invalidate();
            Render();
            if (_noCancelButton != null)
            {
                FocusChild(_noCancelButton);
            }

            Render();
        }).Hidden(() => _confirmingDelete);

        Message(() => _confirmingDelete ? l10n("account.profile.confirm_delete.message") : null);
        Button(l10n("account.profile.confirm_delete.confirm")).OnClick(OnDelete).Hidden(() => !_confirmingDelete);
        _noCancelButton = Button(l10n("account.profile.confirm_delete.cancel")).OnClick(() =>
        {
            _confirmingDelete = false;
            Invalidate();
            Render();
        });
        _noCancelButton.Hidden(() => !_confirmingDelete);

        Button(l10n("account.profile.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
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

        var result = _userLogic.UpdateUser(
            _user.Id,
            username!,
            email!,
            newPassword,
            firstName!,
            lastName!,
            dateOfBirth
        );

        if (!result.Success)
        {
            _fieldErrors = result.FieldErrors;
            return;
        }

        if (_adminUser is null)
        {
            _user.Username = username!;
            _user.Email = email!;
            _user.FirstName = firstName!;
            _user.LastName = lastName!;
            _user.DateOfBirth = dateOfBirth!.Value;
        }
        else
        {
            _user = _adminUser;
        }

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
        _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
}
