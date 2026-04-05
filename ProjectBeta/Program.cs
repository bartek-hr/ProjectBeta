using ProjectBeta.CI;
using ProjectBeta.CI.Views;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Data;
using ProjectBeta.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;


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
            //Display(new DemoView());

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>();
            services.AddScoped<UserService>();
            services.AddScoped<UserView>();
            var provider = services.BuildServiceProvider();

            // Initialize DB
            using var context = provider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            Display(provider.GetRequiredService<UserView>());
            App.Run();
        }
    }
}
