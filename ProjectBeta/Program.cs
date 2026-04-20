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
            BaselineLegacySqliteDatabase(context);
            context.Database.Migrate();

            var loginView = provider.GetRequiredService<LoginView>(); // DI will inject AppLoop here
            // Display the LoginView as entry view
            Display(loginView);
            App.Run();
        }

        private static void BaselineLegacySqliteDatabase(AppDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            if (!HasLegacyTables(connection, transaction))
            {
                transaction.Commit();
                return;
            }

            EnsureMigrationsHistoryTableExists(connection, transaction);

            if (HasAppliedMigrations(connection, transaction))
            {
                transaction.Commit();
                return;
            }

            CreateMoviesTableIfMissing(connection, transaction);
            InsertInitialMigrationRecord(connection, transaction);

            transaction.Commit();
        }

        private static bool HasLegacyTables(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            return TableExists(connection, transaction, "Users")
                || TableExists(connection, transaction, "Bookings")
                || TableExists(connection, transaction, "Receipts");
        }

        private static bool HasAppliedMigrations(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM \"__EFMigrationsHistory\";";

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        private static void EnsureMigrationsHistoryTableExists(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        private static void CreateMoviesTableIfMissing(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS "Movies" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Movies" PRIMARY KEY,
                    "Title" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Genres" TEXT NOT NULL,
                    "Rating" REAL NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        private static void InsertInitialMigrationRecord(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES (@migrationId, @productVersion);
                """;

            var migrationId = command.CreateParameter();
            migrationId.ParameterName = "@migrationId";
            migrationId.Value = "20260419183802_InitialCreate";
            command.Parameters.Add(migrationId);

            var productVersion = command.CreateParameter();
            productVersion.ParameterName = "@productVersion";
            productVersion.Value = "8.0.0";
            command.Parameters.Add(productVersion);

            command.ExecuteNonQuery();
        }

        private static bool TableExists(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction, string tableName)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table' AND name = @tableName;
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }
}
