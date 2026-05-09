using ProjectBeta.CI.Components;
using ProjectBeta.Services;

namespace ProjectBeta.Screens;

public class UserScreen
{
    private readonly UserService _userService;

    public UserScreen(UserService userService)
    {
        _userService = userService;
    }

    public void Show()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine(l10n("legacy.user_screen.heading"));
            Console.WriteLine(l10n("legacy.user_screen.options.add_user"));
            Console.WriteLine(l10n("legacy.user_screen.options.list_users"));
            Console.WriteLine(l10n("legacy.user_screen.options.back"));
            Console.Write(l10n("legacy.user_screen.prompt"));

            switch (Console.ReadLine())
            {
                case "1": AddUser(); break;
                case "2": ListUsers(); break;
                case "0": running = false; break;
                default:
                    Console.WriteLine(l10n("legacy.common.invalid_choice"));
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void AddUser()
    {
        string? statusMessage = null;

        var usernameInput = new InputText(l10n("legacy.user_screen.fields.username.label"))
            .Key("username")
            .Placeholder(l10n("legacy.user_screen.fields.username.placeholder"))
            .Required();
        var passwordInput = new InputText(l10n("legacy.user_screen.fields.password.label"))
            .Key("password")
            .Placeholder(l10n("legacy.user_screen.fields.password.placeholder"))
            .Required()
            .Masked();

        var addUserForm = new Form()
            .Add(new Label(l10n("legacy.user_screen.add_user.heading")))
            .Add(new Label(l10n("legacy.user_screen.add_user.instructions")))
            .Add(usernameInput)
            .Add(passwordInput)
            .Add(new Message(() => statusMessage))
            .Add(new Button(l10n("legacy.user_screen.add_user.actions.create")).OnClick(() =>
            {
                if (string.IsNullOrWhiteSpace(usernameInput.Value))
                {
                    statusMessage = l10n("validation.common.required", new Dictionary<string, string> { ["field"] = usernameInput.Label });
                    return;
                }

                if (string.IsNullOrWhiteSpace(passwordInput.Value))
                {
                    statusMessage = l10n("validation.common.required", new Dictionary<string, string> { ["field"] = passwordInput.Label });
                    return;
                }

                if (_userService.Register(usernameInput.Value, passwordInput.Value))
                {
                    statusMessage = l10n("legacy.user_screen.add_user.status.created");
                    usernameInput.Value = string.Empty;
                    passwordInput.Value = string.Empty;
                    return;
                }

                statusMessage = l10n("legacy.user_screen.add_user.status.username_exists");
            }));

        addUserForm.Display();
    }

    private void ListUsers()
    {
        var users = _userService.GetAllUsers();

        if (users.Count == 0)
        {
            Console.WriteLine(l10n("legacy.user_screen.list.empty"));
        }
        else
        {
            foreach (var u in users)
                Console.WriteLine(l10n("legacy.user_screen.list.item", new Dictionary<string, string>
                {
                    ["username"] = u.Username,
                    ["role"] = u.Role
                }));
        }

        Console.WriteLine(l10n("legacy.common.press_any_key"));
        Console.ReadKey();
    }
}
