using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace ProjectBeta.Data;

internal static class SqliteConnectionStringResolver
{
    private const string ConnectionStringName = "DefaultConnection";
    private const string SettingsFileName = "appsettings.json";
    private const string ProjectFileName = "ProjectBeta.csproj";

    public static string GetResolvedConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(SettingsFileName, optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found in {SettingsFileName}.");
        }

        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' must define a SQLite Data Source.");
        }

        if (builder.DataSource == ":memory:" || Path.IsPathRooted(builder.DataSource))
        {
            return builder.ConnectionString;
        }

        builder.DataSource = Path.GetFullPath(builder.DataSource, GetCanonicalDatabaseRoot());
        return builder.ConnectionString;
    }

    private static string GetCanonicalDatabaseRoot()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var projectFilePath = Path.Combine(currentDirectory.FullName, ProjectFileName);
            if (File.Exists(projectFilePath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
