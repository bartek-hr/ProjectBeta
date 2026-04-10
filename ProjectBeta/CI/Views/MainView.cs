using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Views;
using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectBeta.Logic;
namespace ProjectBeta.CI.Views;

public sealed class MainView : Form
{
    private User _user;
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private readonly AppLoop _appLoop;
    private Dictionary<string, string[]>? _fieldErrors;

    public MainView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _user = new User();
        _appLoop = appLoop;
        _statusMessage = null;
        _fieldErrors = null;
    }
    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        // Display user details
        Heading("Welcome " + _user.Username);
        Label("Email: " + _user.Email);
        Label("Full Name: " + _user.FirstName + " " + _user.LastName);


        string statusMessage = null;
        Button("Movies(TBD)").OnClick(form => { statusMessage = "TBD"; });//Reservation goes within MoviesView
        Button("Settings(TBD)").OnClick(form => { statusMessage = "TBD"; });
        Button("Account Details").OnClick(() =>
        {
            Console.Clear();
            var accountView = _serviceProvider.GetRequiredService<AccountView>();
            accountView.SetUser(_user);
            _appLoop.Display(accountView);
        });
        if (_user.Role == "Admin")
        {
            Button("Rapports(TBD)").OnClick(form => { statusMessage = "TBD"; });
            Button("Users(TBD)").OnClick(form => { statusMessage = "TBD"; });
            Button("Cinemas(TBD)").OnClick(form => { statusMessage = "TBD"; });
        }

        Spacer();
        LogoutButton(_appLoop, _serviceProvider);
    }
}
