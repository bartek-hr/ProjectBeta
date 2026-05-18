using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class DiscountAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private DiscountAccess? _access;

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
        _access = new DiscountAccess(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetActive_ReturnsOnlyActiveDiscounts()
    {
        var result = _access!.GetActive();
        Assert.IsTrue(result.All(d => d.IsActive));
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void Add_PersistsDiscount()
    {
        var before = _access!.GetActive().Count;
        _access.Add(new Discount { Name = "Test Discount", Percentage = 10m, IsActive = true, EffectiveFrom = DateTime.UtcNow });
        Assert.AreEqual(before + 1, _access.GetActive().Count);
    }

    [TestMethod]
    public void Update_ChangesPercentage()
    {
        var discount = _context!.Discounts.First();
        discount.Percentage = 99m;
        _access!.Update(discount);
        var updated = _context.Discounts.Find(discount.Id);
        Assert.AreEqual(99m, updated!.Percentage);
    }

    [TestMethod]
    public void Delete_SetsIsActiveToFalse()
    {
        var discount = _context!.Discounts.First(d => d.IsActive);
        var id = discount.Id;
        _access!.Delete(id);
        var updated = _context.Discounts.Find(id);
        Assert.IsFalse(updated!.IsActive);
    }

    [TestMethod]
    public void Delete_NonExistingId_DoesNotThrow()
    {
        _access!.Delete(9999);
    }

    [TestMethod]
    public void GetActive_AfterDelete_ExcludesSoftDeleted()
    {
        var discount = _context!.Discounts.First(d => d.IsActive);
        _access!.Delete(discount.Id);
        var active = _access.GetActive();
        Assert.IsFalse(active.Any(d => d.Id == discount.Id));
    }
}
