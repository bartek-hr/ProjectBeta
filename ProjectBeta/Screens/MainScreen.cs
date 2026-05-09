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
            Console.WriteLine(l10n("legacy.main_screen.heading"));
            Console.WriteLine(l10n("legacy.main_screen.options.users"));
            Console.WriteLine(l10n("legacy.main_screen.options.exit"));
            Console.Write(l10n("legacy.main_screen.prompt"));

            switch (Console.ReadLine())
            {
                case "1": _userScreen.Show(); break;  // navigate to user screen
                case "0": running = false; break;
                default:
                    Console.WriteLine(l10n("legacy.common.invalid_choice"));
                    Console.ReadKey();
                    break;
            }
        }
    }
}
