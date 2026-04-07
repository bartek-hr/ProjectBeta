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

    public void SetUser(User user)
    {
        _user = user;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading("Account Details");
        Label("Update your account info. Tab to navigate, Shift+Tab to go back.");
        Divider();

        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("General") ? string.Join("\n", _fieldErrors["General"]) : null);

        Label("Account Info");
        TextInput("Username").Placeholder("jane_doe").Required().Min(3).Max(20).Default(_user.Username);
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Username") ? string.Join("\n", _fieldErrors["Username"]) : null);

        TextInput("Email").Placeholder("jane@example.com").Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Must be a valid email address").Default(_user.Email);
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Email") ? string.Join("\n", _fieldErrors["Email"]) : null);

        TextInput("New Password").Placeholder("Leave blank to keep current").Masked();
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("New Password") ? string.Join("\n", _fieldErrors["New Password"]) : null);

        TextInput("First Name").Placeholder("Jane").Required().Default(_user.FirstName);
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("First Name") ? string.Join("\n", _fieldErrors["First Name"]) : null);

        TextInput("Last Name").Placeholder("Doe").Required().Default(_user.LastName);
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Last Name") ? string.Join("\n", _fieldErrors["Last Name"]) : null);

        DateInput("Date of Birth").Required()
            .Min(new DateOnly(1900, 1, 1))
            .Max(DateOnly.FromDateTime(DateTime.Today))
            .Default(_user.DateOfBirth ?? DateOnly.FromDateTime(DateTime.Today));
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Date of Birth") ? string.Join("\n", _fieldErrors["Date of Birth"]) : null);

        Divider();

        Message(() => _statusMessage);
        Button("Save").OnClick(OnSave);
        Button("Delete Account").OnClick(() =>
        {
            _confirmingDelete = true;
            Invalidate();
            Render();
            if (_noCancelButton != null) FocusChild(_noCancelButton);
            Render();
        }).Hidden(() => _confirmingDelete);

        Message(() => _confirmingDelete ? "Are you sure you want to delete your account? This cannot be undone." : null);
        Button("Yes, Delete").OnClick(OnDelete).Hidden(() => !_confirmingDelete);
        _noCancelButton = Button("No, Cancel").OnClick(() =>
        {
            _confirmingDelete = false;
            Invalidate();
            Render();
        });
        _noCancelButton.Hidden(() => !_confirmingDelete);

        Button("Back").OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var username = form.Get<string>("Username");
        var email = form.Get<string>("Email");
        var newPassword = form.Get<string>("New Password");
        var firstName = form.Get<string>("First Name");
        var lastName = form.Get<string>("Last Name");
        var dateOfBirth = form.Get<DateOnly?>("Date of Birth");

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
        }
        else
        {
            // Refresh local user reference
            _user.Username = username!;
            _user.Email = email!;
            _user.FirstName = firstName!;
            _user.LastName = lastName!;
            _user.DateOfBirth = dateOfBirth;

            _statusMessage = "Account updated successfully.";
            _fieldErrors = null;
            Invalidate();
            Render();
            Thread.Sleep(1200);
        }
    }

    private void OnDelete(Form form)
    {
        var result = _userLogic.DeleteUser(_user.Id);

        if (!result.Success)
        {
            _confirmingDelete = false;
            _fieldErrors = result.FieldErrors;
        }
        else
        {
            _statusMessage = "Account deleted. Redirecting to Login.";
            _fieldErrors = null;
            Invalidate();
            Render();
            Thread.Sleep(1500);
            Console.Clear();
            _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
        }
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
}
