using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Data.Sqlite;
using ProjectBeta.Data;

namespace ProjectBeta.Tests.Data;

[TestClass]
[DoNotParallelize]
public class SqliteConnectionStringResolverTests
{
    private string? _originalContent;
    private bool _hadOriginalFile;

    private static string SettingsPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    [TestInitialize]
    public void Setup()
    {
        _hadOriginalFile = File.Exists(SettingsPath);
        _originalContent = _hadOriginalFile ? File.ReadAllText(SettingsPath) : null;
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_hadOriginalFile)
        {
            File.WriteAllText(SettingsPath, _originalContent!);
            return;
        }

        if (File.Exists(SettingsPath))
            File.Delete(SettingsPath);
    }

    [TestMethod]
    public void GetResolvedConnectionString_MissingConnectionString_ThrowsInvalidOperationException()
    {
        WriteAppSettings("""
            {
              "ConnectionStrings": {}
            }
            """);

        var exception = Assert.ThrowsException<InvalidOperationException>(InvokeResolver);
        StringAssert.Contains(exception.Message, "DefaultConnection");
    }

    [TestMethod]
    public void GetResolvedConnectionString_MissingDataSource_ThrowsInvalidOperationException()
    {
        WriteAppSettings("""
            {
              "ConnectionStrings": {
                "DefaultConnection": "Mode=ReadWriteCreate"
              }
            }
            """);

        var exception = Assert.ThrowsException<InvalidOperationException>(InvokeResolver);
        StringAssert.Contains(exception.Message, "Data Source");
    }

    [TestMethod]
    public void GetResolvedConnectionString_MemoryDataSource_PassesThrough()
    {
        WriteAppSettings("""
            {
              "ConnectionStrings": {
                "DefaultConnection": "Data Source=:memory:"
              }
            }
            """);

        var result = new SqliteConnectionStringBuilder(InvokeResolver());
        Assert.AreEqual(":memory:", result.DataSource);
    }

    [TestMethod]
    public void GetResolvedConnectionString_RootedPath_PassesThrough()
    {
        var absolutePath = Path.Combine(Path.GetTempPath(), "projectbeta-tests.db");
        WriteAppSettings($$"""
            {
              "ConnectionStrings": {
                "DefaultConnection": "Data Source={{absolutePath}}"
              }
            }
            """);

        var result = new SqliteConnectionStringBuilder(InvokeResolver());
        Assert.AreEqual(absolutePath, result.DataSource);
    }

    [TestMethod]
    public void GetResolvedConnectionString_RelativePath_ExpandsToAbsolutePath()
    {
        WriteAppSettings("""
            {
              "ConnectionStrings": {
                "DefaultConnection": "Data Source=app.db"
              }
            }
            """);

        var result = new SqliteConnectionStringBuilder(InvokeResolver());
        Assert.AreEqual(Path.GetFullPath("app.db", AppContext.BaseDirectory), result.DataSource);
    }

    private static string InvokeResolver()
    {
        var resolverType = typeof(AppDbContext).Assembly.GetType("ProjectBeta.Data.SqliteConnectionStringResolver")
            ?? throw new AssertFailedException("Could not locate SqliteConnectionStringResolver.");
        var method = resolverType.GetMethod(
            "GetResolvedConnectionString",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertFailedException("Could not locate GetResolvedConnectionString.");

        try
        {
            return (string)method.Invoke(null, null)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static void WriteAppSettings(string content)
    {
        File.WriteAllText(SettingsPath, content);
    }
}
