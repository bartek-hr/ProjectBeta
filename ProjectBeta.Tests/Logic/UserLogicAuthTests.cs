using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests;

[TestClass]
public class UserLogicAuthTests
{
    private AppDbContext? _context;
    private UserAccess? _userAccess;
    private UserLogic? _logic;
    private SqliteConnection? _connection;

    private User SuperAdmin => _context!.Users.Find(1)!;
    private User RegularUser => _context!.Users.Find(2)!;

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

    // --- SearchUser (login) ---

    [TestMethod]
    public void SearchUser_ValidCredentials_ReturnsUser()
    {
        var result = _logic!.SearchUser("admin", "admin@example.com", "password");
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.User);
        Assert.AreEqual("admin", result.User!.Username);
    }

    [TestMethod]
    public void SearchUser_WrongPassword_ReturnsFailure()
    {
        var result = _logic!.SearchUser("admin", "admin@example.com", "wrongpw");
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("identity"));
    }

    [TestMethod]
    public void SearchUser_UnknownUser_ReturnsFailure()
    {
        var result = _logic!.SearchUser("nobody", "nobody@example.com", "pw");
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
    }

    [TestMethod]
    public void SearchUser_MissingUsername_ReturnsValidationFailure()
    {
        var result = _logic!.SearchUser("", "admin@example.com", "password");
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("general"));
    }

    [TestMethod]
    public void SearchUser_MissingPassword_ReturnsValidationFailure()
    {
        var result = _logic!.SearchUser("admin", "admin@example.com", "");
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void SearchUser_NullArgs_ReturnsValidationFailure()
    {
        var result = _logic!.SearchUser(null, null, null);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
    }

    // --- Role changes via UpdateUser ---

    [TestMethod]
    public void UpdateUser_SuperAdminPromotsUserToAdmin_Succeeds()
    {
        var superAdmin = SuperAdmin;
        var regularUser = RegularUser;

        var result = _logic!.UpdateUser(
            regularUser.Id,
            regularUser.Username,
            regularUser.Email,
            null,
            regularUser.FirstName,
            regularUser.LastName,
            regularUser.DateOfBirth,
            superAdmin,
            role: "Admin"
        );

        Assert.IsTrue(result.Success);
        var updated = _context!.Users.Find(regularUser.Id);
        Assert.AreEqual("Admin", updated!.Role);
    }

    [TestMethod]
    public void UpdateUser_NonSuperAdminChangesRole_Fails()
    {
        var regularUser = RegularUser;

        var result = _logic!.UpdateUser(
            regularUser.Id,
            regularUser.Username,
            regularUser.Email,
            null,
            regularUser.FirstName,
            regularUser.LastName,
            regularUser.DateOfBirth,
            regularUser,
            role: "Admin"
        );

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("role"));
    }

    [TestMethod]
    public void UpdateUser_InvalidRole_Fails()
    {
        var superAdmin = SuperAdmin;
        var regularUser = RegularUser;

        var result = _logic!.UpdateUser(
            regularUser.Id,
            regularUser.Username,
            regularUser.Email,
            null,
            regularUser.FirstName,
            regularUser.LastName,
            regularUser.DateOfBirth,
            superAdmin,
            role: "SuperAdmin"
        );

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("role"));
    }

    [TestMethod]
    public void UpdateUser_SuperAdminRoleCannotBeChanged()
    {
        var superAdmin = SuperAdmin;

        var result = _logic!.UpdateUser(
            superAdmin.Id,
            superAdmin.Username,
            superAdmin.Email,
            null,
            superAdmin.FirstName,
            superAdmin.LastName,
            superAdmin.DateOfBirth,
            superAdmin,
            role: "User"
        );

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("role"));
    }

    [TestMethod]
    public void UpdateUser_UserNotFound_ReturnsFailure()
    {
        var superAdmin = SuperAdmin;
        var result = _logic!.UpdateUser(
            9999, "ghost", "ghost@x.com", null, "G", "H",
            new DateOnly(1990, 1, 1), superAdmin
        );
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("general"));
    }

    [TestMethod]
    public void UpdateUser_DuplicateEmail_Fails()
    {
        var superAdmin = SuperAdmin;
        var regularUser = RegularUser;

        var result = _logic!.UpdateUser(
            regularUser.Id,
            regularUser.Username,
            "admin@example.com",
            null,
            regularUser.FirstName,
            regularUser.LastName,
            regularUser.DateOfBirth,
            superAdmin
        );

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("email"));
    }

    // --- Register validation ---

    [TestMethod]
    public void Register_MissingFirstName_ReturnsFailure()
    {
        var result = _logic!.Register("newuser", "newuser@x.com", "pw", "", "Last", new DateOnly(2000, 1, 1));
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void Register_NullDateOfBirth_ReturnsFailure()
    {
        var result = _logic!.Register("newuser", "newuser@x.com", "pw", "First", "Last", null);
        Assert.IsFalse(result.Success);
    }

    // --- GetAllUsers ---

    [TestMethod]
    public void GetAllUsers_ReturnsSeededUsers()
    {
        var result = _logic!.GetAllUsers();
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Users);
        Assert.AreEqual(3, result.Users!.Count);
    }
}
