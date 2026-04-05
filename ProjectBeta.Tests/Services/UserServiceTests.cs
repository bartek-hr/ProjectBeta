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

        var result = service.Register(
            "testuser",
            "testuser@example.com",
            "pw",
            "Test",
            "User",
            new DateOnly(2000, 1, 1)
        );
        Assert.IsTrue(result.Success);
        Assert.IsNull(result.FieldErrors);
        // 3 users in total with seeded users
        Assert.AreEqual(3, context.Users.Count());
    }

    [TestMethod]
    public void Register_ExistingUser_ReturnsFalse()
    {
        var service = _service!;

        // First registration should succeed
        var first = service.Register(
            "testuser",
            "testuser@example.com",
            "pw",
            "Test",
            "User",
            new DateOnly(2000, 1, 1)
        );
        Assert.IsTrue(first.Success);
        Assert.IsNull(first.FieldErrors);

        // Second registration with same username/email should fail
        var second = service.Register(
            "testuser",
            "testuser@example.com",
            "pw2",
            "Test2",
            "User2",
            new DateOnly(2001, 2, 2)
        );
        Assert.IsFalse(second.Success);
        Assert.IsNotNull(second.FieldErrors);
        Assert.IsTrue(second.FieldErrors!.ContainsKey("Username") || second.FieldErrors!.ContainsKey("Email"));
    }
}
