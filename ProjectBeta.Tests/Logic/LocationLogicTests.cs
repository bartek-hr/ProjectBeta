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
        _context!.Locations.Add(new Location { Capacity = 3 });
        _context.Locations.Add(new Location { Capacity = 5 });
        _context.SaveChanges();

        var result = _logic!.GetAll();
        Assert.AreEqual(2, result.Count);
    }

    // --- GetById ---

    [TestMethod]
    public void GetById_ExistingId_ReturnsLocation()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 4 });
        _context.SaveChanges();

        var result = _logic!.GetById(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual(4, result.Capacity);
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
        _context!.Locations.Add(new Location { Id = 10, Capacity = 2 });
        _context.SaveChanges();

        // Assign existing seeded auditorium to this location
        var auditorium = _context.Auditoriums.Find(1)!;
        auditorium.LocationId = 10;
        _context.SaveChanges();

        var result = _logic!.GetById(10);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Auditoriums.Count);
    }

    // --- Add ---

    [TestMethod]
    public void Add_AsAdmin_SavesLocation()
    {
        var location = new Location { Capacity = 6 };

        _logic!.Add(location, AdminUser);

        Assert.AreEqual(1, _context!.Locations.Count());
    }

    [TestMethod]
    public void Add_AsAdmin_CorrectCapacityStored()
    {
        var location = new Location { Capacity = 8 };

        _logic!.Add(location, AdminUser);

        var saved = _context!.Locations.First();
        Assert.AreEqual(8, saved.Capacity);
    }

    [TestMethod]
    public void Add_AsRegularUser_ThrowsUnauthorized()
    {
        var location = new Location { Capacity = 3 };

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Add(location, RegularUser));
    }

    [TestMethod]
    public void Add_AsRegularUser_DoesNotSaveToDatabase()
    {
        var location = new Location { Capacity = 3 };

        try { _logic!.Add(location, RegularUser); } catch { }

        Assert.AreEqual(0, _context!.Locations.Count());
    }

    // --- Delete ---

    [TestMethod]
    public void Delete_AsAdmin_RemovesLocation()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        _logic!.Delete(1, AdminUser);

        Assert.AreEqual(0, _context.Locations.Count());
    }

    [TestMethod]
    public void Delete_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Delete(1, RegularUser));
    }

    [TestMethod]
    public void Delete_AsRegularUser_DoesNotRemoveFromDatabase()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        try { _logic!.Delete(1, RegularUser); } catch { }

        Assert.AreEqual(1, _context.Locations.Count());
    }

    [TestMethod]
    public void Delete_NonExistingId_DoesNotThrow()
    {
        _logic!.Delete(999, AdminUser);
        Assert.AreEqual(0, _context!.Locations.Count());
    }

    // --- UpdateCapacity ---

    [TestMethod]
    public void UpdateCapacity_AsAdmin_UpdatesCapacity()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        _logic!.UpdateCapacity(1, 10, AdminUser);

        var updated = _context.Locations.Find(1);
        Assert.AreEqual(10, updated!.Capacity);
    }

    [TestMethod]
    public void UpdateCapacity_AsRegularUser_ThrowsUnauthorized()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.UpdateCapacity(1, 10, RegularUser));
    }

    [TestMethod]
    public void UpdateCapacity_AsRegularUser_DoesNotChangeCapacity()
    {
        _context!.Locations.Add(new Location { Id = 1, Capacity = 2 });
        _context.SaveChanges();

        try { _logic!.UpdateCapacity(1, 10, RegularUser); } catch { }

        var unchanged = _context.Locations.Find(1);
        Assert.AreEqual(2, unchanged!.Capacity);
    }

    [TestMethod]
    public void UpdateCapacity_NonExistingId_DoesNotThrow()
    {
        _logic!.UpdateCapacity(999, 5, AdminUser);
    }

    // --- Search ---

    [TestMethod]
    public void Search_BlankQuery_ReturnsAllLocations()
    {
        _context!.Locations.AddRange(
            new Location { Name = "Alpha", Capacity = 100 },
            new Location { Name = "Beta",  Capacity = 200 }
        );
        _context.SaveChanges();

        var result = _logic!.Search("   ");
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Search_MatchingQuery_FiltersLocations()
    {
        _context!.Locations.AddRange(
            new Location { Name = "Alpha", Capacity = 100 },
            new Location { Name = "Beta",  Capacity = 200 }
        );
        _context.SaveChanges();

        var result = _logic!.Search("Alpha");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Alpha", result[0].Name);
    }

    [TestMethod]
    public void Search_NoMatch_ReturnsEmpty()
    {
        _context!.Locations.Add(new Location { Name = "Alpha", Capacity = 100 });
        _context.SaveChanges();

        var result = _logic!.Search("Zzzz");
        Assert.AreEqual(0, result.Count);
    }
}
