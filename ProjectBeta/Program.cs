using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI;
using ProjectBeta.CI.Views;
using ProjectBeta.Data;
using ProjectBeta.Logic;

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
            services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://api.imdbapi.dev/") });
            services.AddDbContext<AppDbContext>((_, options) =>
                options.UseSqlite(SqliteConnectionStringResolver.GetResolvedConnectionString()));
            services.AddScoped<UserAccess>();
            services.AddScoped<MovieAccess>();
            services.AddScoped<MovieScheduleAccess>();
            services.AddScoped<UserLogic>();
            services.AddScoped<AuditoriumAccess>();
            services.AddScoped<AuditoriumLogic>();
            services.AddScoped<CinemaAccess>();
            services.AddScoped<CinemaLogic>();
            services.AddScoped<MovieLogic>();
            services.AddScoped<SnackAccess>();
            services.AddScoped<SnackLogic>();
            services.AddScoped<BookingSnackAccess>();
            services.AddScoped<BookingSnackLogic>();
            services.AddTransient<UserView>();
            services.AddTransient<UsersView>();
            services.AddTransient<LoginView>();
            services.AddTransient<AccountView>();
            services.AddTransient<CinemaView>();
            services.AddTransient<CinemaDetailView>();
            services.AddTransient<CinemaDetailView>();
            services.AddTransient<AuditoriumListView>();
            services.AddTransient<AuditoriumEditView>();
            services.AddTransient<MoviesView>();
            services.AddScoped<MainView>();
            var provider = services.BuildServiceProvider();

            // Initialize DB
            using var context = provider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();

            var loginView = provider.GetRequiredService<LoginView>(); // DI will inject AppLoop here
            // Display the LoginView as entry view
            Display(loginView);
            App.Run();
        }
    }
}
