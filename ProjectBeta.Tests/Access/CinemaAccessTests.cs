using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class CinemaAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private CinemaAccess? _access;

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
        _access = new CinemaAccess(_context);
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
        var result = _access!.GetAll();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Darcy", result[0].Name);
    }

    [TestMethod]
    public void GetById_ExistingId_IncludesAuditoriums()
    {
        var result = _access!.GetById(1);
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.Auditoriums.Count > 0);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _access!.GetById(999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UpdateName_ExistingId_UpdatesName()
    {
        _access!.UpdateName(1, "New Name");
        var updated = _context!.Cinemas.Find(1);
        Assert.AreEqual("New Name", updated!.Name);
    }

    [TestMethod]
    public void UpdateName_NonExistingId_DoesNotThrow()
    {
        _access!.UpdateName(999, "Ghost");
    }

    [TestMethod]
    public void UpdateCity_ExistingId_UpdatesCity()
    {
        _access!.UpdateCity(1, "Amsterdam");
        var updated = _context!.Cinemas.Find(1);
        Assert.AreEqual("Amsterdam", updated!.City);
    }

    [TestMethod]
    public void UpdateCity_NonExistingId_DoesNotThrow()
    {
        _access!.UpdateCity(999, "Nowhere");
    }
}
