using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class SeatPriceAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private SeatPriceAccess? _access;

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
        _context.Seed();
        _access = new SeatPriceAccess(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetAll_ReturnsSeededSeatPrices()
    {
        var result = _access!.GetAll();
        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void GetById_ExistingId_ReturnsSeatPrice()
    {
        var result = _access!.GetById(1);
        Assert.IsNotNull(result);
        Assert.AreEqual("Standard", result!.Name);
    }

    [TestMethod]
    public void GetById_NonExistingId_ReturnsNull()
    {
        var result = _access!.GetById(999);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void UpdatePrice_ExistingId_UpdatesPrice()
    {
        _access!.UpdatePrice(1, 99.99m);
        var updated = _context!.SeatPrices.Find(1);
        Assert.AreEqual(99.99m, updated!.Price);
    }

    [TestMethod]
    public void UpdatePrice_NonExistingId_DoesNotThrow()
    {
        _access!.UpdatePrice(9999, 50m);
    }
}
