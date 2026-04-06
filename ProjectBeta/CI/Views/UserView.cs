using ProjectBeta.CI.Components;
using ProjectBeta.Services;

public sealed class UserView : Form
{
    private readonly UserService _userService;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public UserView(UserService userService)
    {
        _userService = userService;
        _statusMessage = null;
        _fieldErrors = null;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading("User Information");
        Label("Please enter user details. Tab to navigate, Shift+Tab to go back, Escape to exit.");
        Divider();

        // General error at the top
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("General") ? string.Join("\n", _fieldErrors["General"]) : null);

        Label("Account Info");
        TextInput("Username").Placeholder("jane_doe").Required().Min(3).Max(20);
        // Username error
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Username") ? string.Join("\n", _fieldErrors["Username"]) : null);

        TextInput("Email").Placeholder("jane@example.com").Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Must be a valid email address");
        // Email error
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Email") ? string.Join("\n", _fieldErrors["Email"]) : null);

        TextInput("Password").Placeholder("Choose a password").Required().Masked();
        // Password error
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Password") ? string.Join("\n", _fieldErrors["Password"]) : null);

        Label("Personal Info");
        TextInput("First Name").Placeholder("Jane").Required();
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("First Name") ? string.Join("\n", _fieldErrors["First Name"]) : null);

        TextInput("Last Name").Placeholder("Doe").Required();
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("Last Name") ? string.Join("\n", _fieldErrors["Last Name"]) : null);

        DateInput("Date of Birth").Required().Min(new DateOnly(1900, 1, 1)).Max(DateOnly.FromDateTime(DateTime.Today));
        Message(() =>
        {
            return _fieldErrors != null && _fieldErrors.ContainsKey("Date of Birth")
                ? string.Join("\n", _fieldErrors["Date of Birth"])
                : null;
        });

        Divider();

        Message(() => _statusMessage);
        Button("Submit").OnClick(OnSubmit);
        Button("Reset").OnClick(OnReset);
    }

    private void OnSubmit(Form form)
    {
        // Clear previous errors
        _fieldErrors = null;
        _statusMessage = null;

        // Get form values
        var username = form.Get<string>("Username");
        var email = form.Get<string>("Email");
        var password = form.Get<string>("Password");
        var firstName = form.Get<string>("First Name");
        var lastName = form.Get<string>("Last Name");
        var dateOfBirth = form.Get<DateOnly?>("Date of Birth");

        // Use injected UserService
        var result = _userService.Register(
            username,
            email,
            password,
            firstName,
            lastName,
            dateOfBirth
        );

        if (!result.Success)
        {
            _fieldErrors = result.FieldErrors;
            _statusMessage = null;
        }
        else
        {
            _statusMessage = "User registered successfully.";
            _fieldErrors = null;
        }
    }

    private void OnReset()
    {
        _statusMessage = "Form reset.";
        _fieldErrors = null;
        //TODO: Clear Form, by setting fields empty
    }
}
