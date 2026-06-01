using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Logic;

[TestClass]
public class LocationLogicTests
{
    private AppDbContext? _context;
    private LocationAccess? _locationAccess;
    private LocationLogic? _logic;
    private SqliteConnection? _connection;

    private User SuperAdminUser => new User
    {
        Id = 97,
        Username = "superadmin_test",
        Role = "SuperAdmin",
        PasswordHash = "x",
        Email = "sa@a.com",
        FirstName = "S",
        LastName = "A",
        DateOfBirth = new DateOnly(1985, 1, 1)
    };

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
        // Clear seeded data so tests start from an empty state
        _context.Auditoriums.RemoveRange(_context.Auditoriums);
        _context.Locations.RemoveRange(_context.Locations);
        _context.SaveChanges();
        _locationAccess = new LocationAccess(_context);
        _logic = new LocationLogic(_locationAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    // --- GetAll ---

    [TestMethod]
    public void GetAll_NoLocations_ReturnsEmptyList()
    {
        var result = _logic!.GetAll();
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAll_AfterAddingLocations_ReturnsAll()
    {
        _context!.Locations.Add(new Location { Name = "A", City = "X", Address = "Y" });
        _context.Locations.Add(new Location { Name = "B", City = "X", Address = "Y" });
        _context.SaveChanges();

        var result = _logic!.GetAll();
        Assert.AreEqual(2, result.Count);
    }

    // --- GetById ---

    [TestMethod]
    public void GetById_ExistingId_ReturnsLocation()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "Main", City = "Rotterdam", Address = "Main St 1" });
        _context.SaveChanges();

        var result = _logic!.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("Rotterdam", result.City);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _logic!.GetById(999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_IncludesAuditoriums()
    {
        _context!.Locations.Add(new Location { Id = 10, Name = "L", City = "C", Address = "A" });
        _context.Auditoriums.Add(new Auditorium { Name = "Aud1", Capacity = 100, LocationId = 10 });
        _context.SaveChanges();

        var result = _logic!.GetById(10);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Auditoriums.Count);
    }

    // --- Add ---

    [TestMethod]
    public void Add_AsSuperAdmin_SavesLocation()
    {
        var location = new Location { Name = "New", City = "City", Address = "Addr" };
        _logic!.Add(location, SuperAdminUser);
        Assert.AreEqual(1, _context!.Locations.Count());
    }

    [TestMethod]
    public void Add_AsAdmin_ThrowsUnauthorized()
    {
        var location = new Location { Name = "New", City = "City", Address = "Addr" };
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Add(location, AdminUser));
    }

    [TestMethod]
    public void Add_AsRegularUser_ThrowsUnauthorized()
    {
        var location = new Location { Name = "New", City = "City", Address = "Addr" };
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Add(location, RegularUser));
    }

    [TestMethod]
    public void Add_AsRegularUser_DoesNotSaveToDatabase()
    {
        var location = new Location { Name = "New", City = "City", Address = "Addr" };
        try { _logic!.Add(location, RegularUser); } catch { }
        Assert.AreEqual(0, _context!.Locations.Count());
    }

    // --- Delete ---

    [TestMethod]
    public void Delete_AsSuperAdmin_RemovesLocation()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "C", Address = "A" });
        _context.SaveChanges();
        _logic!.Delete(1, SuperAdminUser);
        Assert.AreEqual(0, _context.Locations.Count());
    }

    [TestMethod]
    public void Delete_AsAdmin_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "C", Address = "A" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Delete(1, AdminUser));
    }

    [TestMethod]
    public void Delete_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "C", Address = "A" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Delete(1, RegularUser));
    }

    [TestMethod]
    public void Delete_NonExistingId_DoesNotThrow()
    {
        _logic!.Delete(999, SuperAdminUser);
        Assert.AreEqual(0, _context!.Locations.Count());
    }

    // --- UpdateName ---

    [TestMethod]
    public void UpdateName_AsSuperAdmin_UpdatesName()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "Old", City = "C", Address = "A" });
        _context.SaveChanges();
        _logic!.UpdateName(1, "New Name", SuperAdminUser);
        var updated = _context.Locations.Find(1);
        Assert.AreEqual("New Name", updated!.Name);
    }

    [TestMethod]
    public void UpdateName_AsAdmin_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "Old", City = "C", Address = "A" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateName(1, "New", AdminUser));
    }

    [TestMethod]
    public void UpdateName_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "Old", City = "C", Address = "A" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateName(1, "New", RegularUser));
    }

    // --- UpdateCity ---

    [TestMethod]
    public void UpdateCity_AsSuperAdmin_UpdatesCity()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "OldCity", Address = "A" });
        _context.SaveChanges();
        _logic!.UpdateCity(1, "NewCity", SuperAdminUser);
        var updated = _context.Locations.Find(1);
        Assert.AreEqual("NewCity", updated!.City);
    }

    [TestMethod]
    public void UpdateCity_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "OldCity", Address = "A" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateCity(1, "NewCity", RegularUser));
    }

    // --- UpdateAddress ---

    [TestMethod]
    public void UpdateAddress_AsSuperAdmin_UpdatesAddress()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "C", Address = "Old Addr" });
        _context.SaveChanges();
        _logic!.UpdateAddress(1, "New Addr", SuperAdminUser);
        var updated = _context.Locations.Find(1);
        Assert.AreEqual("New Addr", updated!.Address);
    }

    [TestMethod]
    public void UpdateAddress_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Name = "L", City = "C", Address = "Old Addr" });
        _context.SaveChanges();
        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateAddress(1, "New Addr", RegularUser));
    }

    // --- Search ---

    [TestMethod]
    public void Search_BlankQuery_ReturnsAllLocations()
    {
        _context!.Locations.AddRange(
            new Location { Name = "Alpha", City = "C", Address = "A" },
            new Location { Name = "Beta", City = "C", Address = "A" }
        );
        _context.SaveChanges();

        var result = _logic!.Search("");
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Search_MatchingQuery_FiltersLocations()
    {
        _context!.Locations.AddRange(
            new Location { Name = "Alpha", City = "C", Address = "A" },
            new Location { Name = "Beta", City = "C", Address = "A" }
        );
        _context.SaveChanges();

        var result = _logic!.Search("Alpha");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Alpha", result[0].Name);
    }

    [TestMethod]
    public void Search_NoMatch_ReturnsEmpty()
    {
        _context!.Locations.Add(new Location { Name = "Alpha", City = "C", Address = "A" });
        _context.SaveChanges();

        var result = _logic!.Search("ZZZZ");
        Assert.AreEqual(0, result.Count);
    }

}
