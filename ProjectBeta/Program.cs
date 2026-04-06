using ProjectBeta.CI;
using ProjectBeta.CI.Views;

namespace ProjectBeta
{
    internal static class Program
    {
        private static readonly AppLoop App = new();

        public static void Display(TerminalInterface terminalInterface)
        {
            App.Display(terminalInterface);
        }

        private static void Main(string[] args)
        {
            Console.Clear();
            Display(new MovieSeatBookingView());
            App.Run();
        }
    }
}
