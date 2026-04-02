using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Services;

namespace ProjectBeta.Tests;

[TestClass]
public class UserServiceTests
{
    private AppDbContext? _context;
    private UserService? _service;
    private SqliteConnection? _connection;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _connection.Open();
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _service = new UserService(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void Register_NewUser_ReturnsTrue()
    {
        var service = _service!;
        var context = _context!;

        var result = service.Register("testuser", "pw");
        Assert.IsTrue(result);
        // 3 user in total with seeded users
        Assert.AreEqual(3, context.Users.Count());
    }

    [TestMethod]
    public void Register_ExistingUser_ReturnsFalse()
    {
        var service = _service!;

        service.Register("testuser", "pw");
        var result = service.Register("testuser", "pw2");
        Assert.IsFalse(result);
    }
}
