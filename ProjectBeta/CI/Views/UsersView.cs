using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class UsersView : Form
{
    private readonly UserLogic _userLogic;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user = null!;
    private List<User> _users = [];
    private string? _statusMessage;

    public UsersView(UserLogic userLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _userLogic = userLogic;
        _serviceProvider = serviceProvider;
        _appLoop = appLoop;
    }

    public void SetUser(User user)
    {
        _user = user;
        LoadUsers();
        ClearChildren();
        InitializeForm();
    }

    private void LoadUsers()
    {
        var result = _userLogic.GetAllUsers();
        _users = result.Users ?? [];
        _statusMessage = result.FieldErrors != null &&
                         result.FieldErrors.TryGetValue("general", out var errors)
            ? string.Join(Environment.NewLine, errors)
            : null;
    }

    private void InitializeForm()
    {
        Heading(l10n("admin.users.heading"));
        Divider();
        Message(() => _statusMessage);

        var table = Table<User>(
                l10n("admin.users.table.username"),
                l10n("admin.users.table.name"),
                l10n("admin.users.table.email"),
                l10n("admin.users.table.role"))
            .EmptyMessage(_statusMessage ?? l10n("admin.users.empty"))
            .OnSelect(OpenAccount);

        foreach (var userAccount in _users)
        {
            table.AddRow(
                userAccount,
                userAccount.Username,
                $"{userAccount.FirstName} {userAccount.LastName}",
                userAccount.Email,
                userAccount.Role
            );
        }

        Divider();
        Button(l10n("admin.users.actions.back")).OnClick(NavigateToMain);
    }

    private void OpenAccount(User userAccount)
    {
        Console.Clear();
        var accountView = _serviceProvider.GetRequiredService<AccountView>();
        accountView.SetUser(userAccount, _user);
        _appLoop.Display(accountView);
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
}
