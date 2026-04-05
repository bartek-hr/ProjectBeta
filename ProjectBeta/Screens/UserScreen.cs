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
            Console.WriteLine("=== User Menu ===");
            Console.WriteLine("1. Add user");
            Console.WriteLine("2. List users");
            Console.WriteLine("0. Back");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1": AddUser(); break;
                case "2": ListUsers(); break;
                case "0": running = false; break;
                default:
                    Console.WriteLine("Invalid choice. Press any key...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void AddUser()
    {
        string? statusMessage = null;

        var usernameInput = new InputText("Username").Placeholder("Enter username").Required();
        var passwordInput = new InputText("Password").Placeholder("Enter password").Required().Masked();

        var addUserForm = new Form()
            .Add(new Label("=== Add User ==="))
            .Add(new Label("Tab moves focus. Enter submits. Escape returns to the menu."))
            .Add(usernameInput)
            .Add(passwordInput)
            .Add(new Message(() => statusMessage))
            .Add(new Button("Create User").OnClick(() =>
            {
                if (string.IsNullOrWhiteSpace(usernameInput.Value))
                {
                    statusMessage = "Username is required.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(passwordInput.Value))
                {
                    statusMessage = "Password is required.";
                    return;
                }

                if (_userService.Register(usernameInput.Value, passwordInput.Value))
                {
                    statusMessage = "User added! Press Escape to return or keep editing to add another user.";
                    usernameInput.Value = string.Empty;
                    passwordInput.Value = string.Empty;
                    return;
                }

                statusMessage = "Username already exists.";
            }));

        addUserForm.Display();
    }

    private void ListUsers()
    {
        var users = _userService.GetAllUsers();

        if (users.Count == 0)
        {
            Console.WriteLine("No users found.");
        }
        else
        {
            foreach (var u in users)
                Console.WriteLine($"- {u.Username} ({u.Role})");
        }

        Console.WriteLine("Press any key...");
        Console.ReadKey();
    }
}
