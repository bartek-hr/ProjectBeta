using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Logic;

namespace ProjectBeta.CI.Views;

public sealed class MainView : Form
{
    private User _user;
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;

    public MainView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _user = new User();
        _appLoop = appLoop;
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

        Button("Movies").OnClick(() => _appLoop.Display(_serviceProvider.GetRequiredService<MoviesView>()));
        
        Button("Settings(TBD)").OnClick(form => { });
        Button("Account Details").OnClick(() =>
        {
            Console.Clear();
            var accountView = _serviceProvider.GetRequiredService<AccountView>();
            accountView.SetUser(_user);
            _appLoop.Display(accountView);
        });
        if (_user.Role == "Admin")
        {
            Button("Rapports(TBD)").OnClick(form => { });
            Button("Users(TBD)").OnClick(form => { });
            Button("Cinemas(TBD)").OnClick(form => { });
        }

        Spacer();
        LogoutButton(_appLoop, _serviceProvider);
    }
}
