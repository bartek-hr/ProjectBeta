using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Views;
using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectBeta.Logic;
using System.Reflection.Metadata;
namespace ProjectBeta.CI.Views;

public sealed class UsersView : Form
{
    private User _user;
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private string? _statusMessage;
    private readonly AppLoop _appLoop;
    private List<User>? _users = [];

    public UsersView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _users = _userLogic.GetAllUsers().Users;
        _user = new User();
        _appLoop = appLoop;
        _statusMessage = null;
    }
    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        // Display users details
        foreach (User userAccount in _users ?? [])
        {
            Divider();
            Button($"{userAccount.Username} | {userAccount.FirstName} {userAccount.LastName} | {userAccount.Email}").OnClick(() =>
            {
                Console.Clear();
                var accountView = _serviceProvider.GetRequiredService<AccountView>();
                accountView.SetUser(userAccount, _user);
                _appLoop.Display(accountView);
            });
            Divider();
        }

        Divider();
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }
}
