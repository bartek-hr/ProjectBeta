using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Views;
using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Logic;

public sealed class LoginView : Form
{
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private readonly AppLoop _appLoop;
    private Dictionary<string, string[]>? _fieldErrors;

    public LoginView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _appLoop = appLoop;
        _statusMessage = null;
        _fieldErrors = null;
        InitializeForm();
    }
    private void InitializeForm(bool loggingAgain = false)
    {
        Label("Please register or login. Tab to navigate, Shift+Tab to go back, Escape to exit.");
        Button("login").OnClick(OnLogin);
        Button("Register").OnClick(OnRegister);
    }
    private void InitializeLogin(bool loggingAgain = false)
    {
        Heading("Login");
        if (loggingAgain)
        {
            Label("One of credentials is wrong please try again. Tab to navigate, Shift+Tab to go back, Escape to exit.");
        }
        else
        {
            Label("Please enter credentials. Tab to navigate, Shift+Tab to go back, Escape to exit.");
        }
        Divider();

        // General error at the top
        Message(() => _fieldErrors != null && _fieldErrors.ContainsKey("General") ? string.Join("\n", _fieldErrors["General"]) : null);

        Label("Log In");
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



        Divider();

        Message(() => _statusMessage);
        Button("Submit").OnClick(OnSubmit);
        Button("Reset").OnClick(OnReset);
    }
    private void OnLogin(Form form)
    {
        InitializeLogin(false);
    }
    private void OnRegister(Form form)
    {
        NavigateToUserView();
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

        // Use injected UserService
        var result = _userLogic.SearchUser(username, email, password);

        if (!result.Success)
        {
            Console.Clear();

            _fieldErrors = result.FieldErrors;
            _statusMessage = "Login failed. Please try again.";
            System.Threading.Thread.Sleep(2000);
            InitializeLogin(true);
        }
        else
        {
            _statusMessage = $"Hi {result.User!.Username}.";
            _fieldErrors = null;

            // After successful login, go to MainView or another screen
            NavigateToMainView(result.User);
        }
    }

    private void NavigateToMainView(User user)
    {
        // Clear the console before displaying the next screen
        Console.Clear();

        // Get the MainView from the DI container
        var mainView = _serviceProvider.GetRequiredService<MainView>(); // This line should work now

        // Pass the User to MainView if needed
        mainView.SetUser(user);

        // Change the current view to MainView
        _appLoop.Display(mainView);
    }

    private void NavigateToUserView()
    {
        // Clear the console before displaying the next screen
        Console.Clear();

        // Get the UserView from the DI container

        // Change the current view to UserView
        _appLoop.Display(_serviceProvider.GetRequiredService<UserView>());
    }

    private void OnReset()
    {
        _statusMessage = "Form reset.";
        _fieldErrors = null;
        //TODO: Clear Form, by setting fields empty
    }
}
