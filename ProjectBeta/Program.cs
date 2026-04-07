using ProjectBeta.CI;
using ProjectBeta.CI.Views;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectBeta.Logic;
using ProjectBeta.Access;


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
            var services = new ServiceCollection();
            services.AddSingleton(App);
            services.AddDbContext<AppDbContext>();
            services.AddScoped<UserAccess>();
            services.AddScoped<UserLogic>();
            services.AddTransient<UserView>();
            services.AddTransient<LoginView>();
            services.AddTransient<AccountView>();
            services.AddScoped<MainView>();
            var provider = services.BuildServiceProvider();

            // Initialize DB
            using var context = provider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            var loginView = provider.GetRequiredService<LoginView>(); // DI will inject AppLoop here
            // Display the LoginView as entry view
            Display(loginView);
            App.Run();
        }
    }
}
