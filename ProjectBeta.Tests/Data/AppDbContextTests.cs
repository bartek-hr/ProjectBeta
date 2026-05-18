using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;

namespace ProjectBeta.Tests.Data;

[TestClass]
public class AppDbContextTests
{
    private AppDbContext? _context;
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
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    // --- Seed idempotency ---

    [TestMethod]
    public void Seed_PopulatesDiscounts()
    {
        _context!.Seed();
        Assert.IsTrue(_context.Discounts.Any());
    }

    [TestMethod]
    public void Seed_IsIdempotent_DoesNotDuplicateDiscounts()
    {
        _context!.Seed();
        var firstCount = _context.Discounts.Count();
        _context.Seed();
        Assert.AreEqual(firstCount, _context.Discounts.Count());
    }

    [TestMethod]
    public void Seed_PopulatesSeatPrices()
    {
        _context!.Seed();
        Assert.IsTrue(_context.SeatPrices.Any());
    }

    [TestMethod]
    public void Seed_IsIdempotent_DoesNotDuplicateSeatPrices()
    {
        _context!.Seed();
        var firstCount = _context.SeatPrices.Count();
        _context.Seed();
        Assert.AreEqual(firstCount, _context.SeatPrices.Count());
    }

    [TestMethod]
    public void Seed_DefaultDiscountsHaveExpectedNames()
    {
        _context!.Seed();
        var names = _context.Discounts.Select(d => d.Name).ToList();
        CollectionAssert.Contains(names, "Child Discount");
        CollectionAssert.Contains(names, "Senior Discount");
        CollectionAssert.Contains(names, "Group Discount");
    }

    [TestMethod]
    public void Seed_DefaultSeatPricesHaveExpectedNames()
    {
        _context!.Seed();
        var names = _context.SeatPrices.Select(s => s.Name).ToList();
        CollectionAssert.Contains(names, "Standard");
        CollectionAssert.Contains(names, "VIP");
        CollectionAssert.Contains(names, "King");
    }

    // --- EnsureCreated data contract ---

    [TestMethod]
    public void EnsureCreated_SeedsDefaultUsers()
    {
        Assert.AreEqual(2, _context!.Users.Count());
    }

    [TestMethod]
    public void EnsureCreated_SeedsDefaultCinema()
    {
        Assert.AreEqual(1, _context!.Cinemas.Count());
        Assert.AreEqual("Darcy", _context.Cinemas.First().Name);
    }

    [TestMethod]
    public void EnsureCreated_SeedsDefaultAuditoriums()
    {
        Assert.AreEqual(3, _context!.Auditoriums.Count());
    }

    [TestMethod]
    public void EnsureCreated_AdminUserHasSuperAdminRole()
    {
        var admin = _context!.Users.First(u => u.Username == "admin");
        Assert.AreEqual("SuperAdmin", admin.Role);
    }
}
