namespace ProjectBeta.Screens;

public class MainScreen
{
    private readonly UserScreen _userScreen;

    public MainScreen(UserScreen userScreen)
    {
        _userScreen = userScreen;
    }

    public void Show()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("=== Main Menu ===");
            Console.WriteLine("1. Users");
            Console.WriteLine("0. Exit");
            Console.Write("Choice: ");

            switch (Console.ReadLine())
            {
                case "1": _userScreen.Show(); break;  // navigate to user screen
                case "0": running = false; break;
                default:
                    Console.WriteLine("Invalid choice. Press any key...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}
