using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests;

[TestClass]
public class AuditoriumLogicTests
{
    private AppDbContext? _context;
    private AuditoriumAccess? _auditoriumAccess;
    private AuditoriumLogic? _logic;
    private SqliteConnection? _connection;

    private User AdminUser => new User
    {
        Id = 99,
        Username = "admin_test",
        Role = "Admin",
        PasswordHash = "x",
        Email = "a@a.com",
        FirstName = "A",
        LastName = "B",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };
    private User RegularUser => new User
    {
        Id = 98,
        Username = "regular_test",
        Role = "User",
        PasswordHash = "x",
        Email = "b@b.com",
        FirstName = "C",
        LastName = "D",
        DateOfBirth = new DateOnly(1995, 1, 1)
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
        _auditoriumAccess = new AuditoriumAccess(_context);
        _logic = new AuditoriumLogic(_auditoriumAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetAll_ReturnsSeededAuditoriums()
    {
        var result = _logic!.GetAll();
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsAuditorium()
    {
        var result = _logic!.GetById(1);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _logic!.GetById(999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetByCinemaId_NonExistingCinema_ReturnsEmptyList()
    {
        var result = _logic!.GetByCinemaId(999);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void UpdateName_AsAdmin_UpdatesName()
    {
        _logic!.UpdateName(1, "New Name", AdminUser);
        var updated = _context!.Auditoriums.Find(1);
        Assert.AreEqual("New Name", updated!.Name);
    }

    [TestMethod]
    public void UpdateName_AsNonAdmin_ThrowsUnauthorized()
    {
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateName(1, "New Name", RegularUser));
    }

    [TestMethod]
    public void GetByCinemaId_ExistingCinema_ReturnsAuditoriums()
    {
        var result = _logic!.GetByCinemaId(1);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(result.All(a => a.CinemaId == 1));
    }

    [TestMethod]
    public void UpdateCapacity_AsAdmin_UpdatesCapacity()
    {
        _logic!.UpdateCapacity(1, 999, AdminUser);
        var updated = _context!.Auditoriums.Find(1);
        Assert.AreEqual(999, updated!.Capacity);
    }

    [TestMethod]
    public void UpdateCapacity_AsNonAdmin_ThrowsUnauthorized()
    {
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateCapacity(1, 999, RegularUser));
    }

    [TestMethod]
    public void UpdateCinema_AsAdmin_UpdatesCinemaId()
    {
        _context!.Cinemas.Add(new Cinema { Id = 2, Name = "Second", City = "Utrecht" });
        _context.SaveChanges();

        _logic!.UpdateCinema(1, 2, AdminUser);
        var updated = _context.Auditoriums.Find(1);
        Assert.AreEqual(2, updated!.CinemaId);
    }

    [TestMethod]
    public void UpdateCinema_AsNonAdmin_ThrowsUnauthorized()
    {
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateCinema(1, 1, RegularUser));
    }
}
