using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class UserAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private UserAccess? _access;

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
        _access = new UserAccess(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void UsernameExists_KnownUsername_ReturnsTrue()
    {
        Assert.IsTrue(_access!.UsernameExists("admin"));
    }

    [TestMethod]
    public void UsernameExists_UnknownUsername_ReturnsFalse()
    {
        Assert.IsFalse(_access!.UsernameExists("nobody"));
    }

    [TestMethod]
    public void EmailExists_KnownEmail_ReturnsTrue()
    {
        Assert.IsTrue(_access!.EmailExists("admin@example.com"));
    }

    [TestMethod]
    public void EmailExists_UnknownEmail_ReturnsFalse()
    {
        Assert.IsFalse(_access!.EmailExists("nope@x.com"));
    }

    [TestMethod]
    public void FindUserByUsernameOrEmail_ByUsername_ReturnsUser()
    {
        var result = _access!.FindUserByUsernameOrEmail("admin", "does.not@exist.com");
        Assert.IsNotNull(result);
        Assert.AreEqual("admin", result!.Username);
    }

    [TestMethod]
    public void FindUserByUsernameOrEmail_ByEmail_ReturnsUser()
    {
        var result = _access!.FindUserByUsernameOrEmail("nobody", "admin@example.com");
        Assert.IsNotNull(result);
        Assert.AreEqual("admin", result!.Username);
    }

    [TestMethod]
    public void FindUserByUsernameOrEmail_NoMatch_ReturnsNull()
    {
        var result = _access!.FindUserByUsernameOrEmail("x", "x@x.com");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UsernameExistsForOther_SameUser_ReturnsFalse()
    {
        Assert.IsFalse(_access!.UsernameExistsForOther("admin", 1));
    }

    [TestMethod]
    public void UsernameExistsForOther_DifferentUser_ReturnsTrue()
    {
        Assert.IsTrue(_access!.UsernameExistsForOther("admin", 2));
    }

    [TestMethod]
    public void EmailExistsForOther_SameUser_ReturnsFalse()
    {
        Assert.IsFalse(_access!.EmailExistsForOther("admin@example.com", 1));
    }

    [TestMethod]
    public void EmailExistsForOther_DifferentUser_ReturnsTrue()
    {
        Assert.IsTrue(_access!.EmailExistsForOther("admin@example.com", 2));
    }

    [TestMethod]
    public void GetAllUsers_ReturnsAllSeededUsers()
    {
        var result = _access!.GetAllUsers();
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void GetUserById_ExistingId_ReturnsUser()
    {
        var result = _access!.GetUserById(1);
        Assert.IsNotNull(result);
        Assert.AreEqual("admin", result!.Username);
    }

    [TestMethod]
    public void GetUserById_NonExistingId_ReturnsNull()
    {
        var result = _access!.GetUserById(9999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void AddUser_PersistsUser()
    {
        var newUser = new User
        {
            Username = "testuser", Email = "test@test.com", PasswordHash = "pw",
            FirstName = "Test", LastName = "User", DateOfBirth = new DateOnly(2000, 1, 1)
        };
        _access!.AddUser(newUser);
        Assert.AreEqual(4, _context!.Users.Count());
    }

    [TestMethod]
    public void UpdateUser_PersistsChanges()
    {
        var user = _context!.Users.Find(2)!;
        user.FirstName = "Updated";
        _access!.UpdateUser(user);
        Assert.AreEqual("Updated", _context.Users.Find(2)!.FirstName);
    }

    [TestMethod]
    public void DeleteUser_RemovesUser()
    {
        var user = _context!.Users.Find(2)!;
        _access!.DeleteUser(user);
        Assert.IsNull(_context.Users.Find(2));
    }
}
