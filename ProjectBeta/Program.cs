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
            services.AddScoped<BookingAccess>();
            services.AddScoped<MovieScheduleAccess>();
            services.AddScoped<UserLogic>();
            services.AddScoped<AuditoriumAccess>();
            services.AddScoped<AuditoriumLogic>();
            services.AddScoped<MovieLogic>();
            services.AddScoped<SnackAccess>();
            services.AddScoped<SnackLogic>();
            services.AddScoped<BookingSnackAccess>();
            services.AddScoped<BookingSnackLogic>();
            services.AddScoped<DiscountAccess>();
            services.AddScoped<SeatPriceAccess>();
            services.AddScoped<ReceiptAccess>();
            services.AddScoped<ReceiptLogic>();
            services.AddScoped<BookingLogic>();
            services.AddScoped<PricingLogic>();
            services.AddScoped<LocationAccess>();
            services.AddScoped<LocationLogic>();
            services.AddTransient<UserView>();
            services.AddTransient<UsersView>();
            services.AddTransient<MovieSeatBookingView>();
            services.AddTransient<ReservationView>();
            services.AddTransient<ReceiptView>();
            services.AddTransient<SnacksView>();
            services.AddTransient<LoginView>();
            services.AddTransient<AccountView>();
            services.AddTransient<ReservationEditView>();
            services.AddTransient<SnackEditView>();
            services.AddTransient<SnackCreatorView>();
            services.AddTransient<BookingSnacksView>();
            services.AddTransient<ReservationHistoryView>();
            services.AddTransient<UpcomingReservationsView>();
            services.AddTransient<MoviesView>();
            services.AddTransient<SeatPriceView>();
            services.AddTransient<DiscountView>();
            services.AddTransient<LocationView>();
            services.AddTransient<LocationPickerView>();
            services.AddTransient<LocationDetailView>();
            services.AddTransient<LocationEditView>();
            services.AddScoped<MainView>();
            var provider = services.BuildServiceProvider();

            // Initialize DB
            using var context = provider.GetRequiredService<AppDbContext>();

            context.Database.Migrate();
            context.Seed();

            var loginView = provider.GetRequiredService<LoginView>();
            // Display the LoginView as entry view
            Display(loginView);
            App.Run();
        }
    }
}
