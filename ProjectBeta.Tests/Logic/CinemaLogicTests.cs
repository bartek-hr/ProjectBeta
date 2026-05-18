using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests;

[TestClass]
public class CinemaLogicTests
{
    private AppDbContext? _context;
    private CinemaAccess? _cinemaAccess;
    private CinemaLogic? _logic;
    private SqliteConnection? _connection;

    private User AdminUser => new()
    {
        Id = 99, Username = "admin_test", Role = "Admin",
        PasswordHash = "x", Email = "a@a.com",
        FirstName = "A", LastName = "B", DateOfBirth = new DateOnly(1990, 1, 1)
    };

    private User RegularUser => new()
    {
        Id = 98, Username = "regular_test", Role = "User",
        PasswordHash = "x", Email = "b@b.com",
        FirstName = "C", LastName = "D", DateOfBirth = new DateOnly(1995, 1, 1)
    };

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
        _cinemaAccess = new CinemaAccess(_context);
        _logic = new CinemaLogic(_cinemaAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetAll_ReturnsSeededCinema()
    {
        var result = _logic!.GetAll();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Darcy", result[0].Name);
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsCinema()
    {
        var result = _logic!.GetById(1);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
    }

    [TestMethod]
    public void GetById_IncludesAuditoriums()
    {
        var result = _logic!.GetById(1);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Auditoriums.Count > 0);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _logic!.GetById(999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UpdateName_AsAdmin_UpdatesName()
    {
        _logic!.UpdateName(1, "New Name", AdminUser);
        var updated = _context!.Cinemas.Find(1);
        Assert.AreEqual("New Name", updated!.Name);
    }

    [TestMethod]
    public void UpdateName_AsRegularUser_ThrowsUnauthorized()
    {
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateName(1, "New Name", RegularUser));
    }

    [TestMethod]
    public void UpdateName_NonExistingId_DoesNotThrow()
    {
        _logic!.UpdateName(999, "Ghost", AdminUser);
    }

    [TestMethod]
    public void UpdateCity_AsAdmin_UpdatesCity()
    {
        _logic!.UpdateCity(1, "Amsterdam", AdminUser);
        var updated = _context!.Cinemas.Find(1);
        Assert.AreEqual("Amsterdam", updated!.City);
    }

    [TestMethod]
    public void UpdateCity_AsRegularUser_ThrowsUnauthorized()
    {
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateCity(1, "Amsterdam", RegularUser));
    }
}
