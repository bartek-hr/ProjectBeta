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
        Console.Write("Username: ");
        string username = Console.ReadLine()!;

        Console.Write("Password: ");
        string password = Console.ReadLine()!;

        bool success = _userService.Register(username, password);

        if (success)
            Console.WriteLine("User added!");
        else
            Console.WriteLine("Username already exists.");

        Console.WriteLine("Press any key...");
        Console.ReadKey();
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
