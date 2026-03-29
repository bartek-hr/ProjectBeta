using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Data;
using ProjectBeta.Screens;
using ProjectBeta.Services;

namespace ProjectBeta
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Register services and db context 
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>();
            services.AddScoped<UserService>();
            services.AddScoped<UserScreen>();
            services.AddScoped<MainScreen>();

            var provider = services.BuildServiceProvider();

            // Initialize DB
            using var context = provider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            // Start the app
            var mainMenu = provider.GetRequiredService<MainScreen>();
            mainMenu.Show();
        }
    }
}
