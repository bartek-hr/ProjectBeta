using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;

namespace ProjectBeta.Tests;

[TestClass]
public class UserLogicTests
{
    private AppDbContext? _context;
    private UserAccess? _userAccess;
    private UserLogic? _logic;
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
        _userAccess = new UserAccess(_context);
        _logic = new UserLogic(_userAccess);
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
        var service = _logic!;
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
        var service = _logic!;

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
        Assert.IsTrue(second.FieldErrors!.ContainsKey("username") || second.FieldErrors!.ContainsKey("email"));
    }

    [TestMethod]
    public void UpdateUser_ValidData_ReturnsTrue()
    {
        var logic = _logic!;
        var context = _context!;

        // Use seeded user with Id=1
        var result = logic.UpdateUser(
            1,
            "admin_updated",
            "admin_updated@example.com",
            null,
            "Admin",
            "Updated",
            new DateOnly(1990, 1, 1)
        );

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.FieldErrors);

        var user = context.Users.Find(1);
        Assert.AreEqual("admin_updated", user!.Username);
        Assert.AreEqual("admin_updated@example.com", user.Email);
        Assert.AreEqual("Updated", user.LastName);
    }

    [TestMethod]
    public void UpdateUser_DuplicateUsername_ReturnsFalse()
    {
        var logic = _logic!;

        // user with ID 2 tries to take admin's username
        var result = logic.UpdateUser(
            2,
            "admin",
            "user1@example.com",
            null,
            "User",
            "One",
            new DateOnly(1995, 5, 15)
        );

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("username"));
    }

    [TestMethod]
    public void UpdateUser_NewPassword_UpdatesPasswordHash()
    {
        var logic = _logic!;
        var context = _context!;

        var result = logic.UpdateUser(
            1,
            "admin",
            "admin@example.com",
            "newpassword",
            "Admin",
            "User",
            new DateOnly(1990, 1, 1)
        );

        Assert.IsTrue(result.Success);
        var user = context.Users.Find(1);
        Assert.AreEqual("newpassword", user!.PasswordHash);
    }

    [TestMethod]
    public void UpdateUser_BlankPassword_KeepsExistingPasswordHash()
    {
        var logic = _logic!;
        var context = _context!;

        var originalHash = context.Users.Find(1)!.PasswordHash;

        var result = logic.UpdateUser(
            1,
            "admin",
            "admin@example.com",
            "",
            "Admin",
            "User",
            new DateOnly(1990, 1, 1)
        );

        Assert.IsTrue(result.Success);
        var user = context.Users.Find(1);
        Assert.AreEqual(originalHash, user!.PasswordHash);
    }

    [TestMethod]
    public void DeleteUser_ExistingUser_ReturnsTrue()
    {
        var logic = _logic!;
        var context = _context!;

        var result = logic.DeleteUser(1);

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.FieldErrors);
        Assert.IsNull(context.Users.Find(1));
    }

    [TestMethod]
    public void DeleteUser_NonExistingUser_ReturnsFalse()
    {
        var logic = _logic!;

        var result = logic.DeleteUser(9999);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("general"));
    }
}
