using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Logic;

[TestClass]
public class PricingLogicTests
{
    private AppDbContext _context = null!;
    private SqliteConnection _connection = null!;
    private PricingLogic _logic = null!;

    // Seat type IDs matching seed data
    private const int Standard = 1;
    private const int Vip      = 2;
    private const int King     = 3;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _context.Seed();

        var discountAccess = new DiscountAccess(_context);
        var seatAccess = new SeatPriceAccess(_context);
        _logic = new PricingLogic(discountAccess, seatAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [TestMethod]
    public void Adult_Standard_NoDiscount_ReturnsFullPrice()
    {
        // Age 30, 1 seat
        var result = _logic.CalculatePricing([Standard], [30], DateTime.UtcNow);

        Assert.AreEqual(15.00m, result.BasePrice);
        Assert.AreEqual(15.00m, result.FinalPrice);
        Assert.AreEqual(0, result.Discounts.Count);
        Assert.IsNull(result.SeatLines[0].Discount);
    }

    [TestMethod]
    public void Child_Age12_Standard_Gets50PercentOff()
    {
        var result = _logic.CalculatePricing([Standard], [12], DateTime.UtcNow);
        Assert.AreEqual(7.50m, result.FinalPrice);
    }

    [TestMethod]
    public void Child_Age13_Standard_NoDiscount()
    {
        var result = _logic.CalculatePricing([Standard], [13], DateTime.UtcNow);

        Assert.AreEqual(15.00m, result.FinalPrice);
        Assert.AreEqual(0, result.Discounts.Count);
    }

    [TestMethod]
    public void NoAgeProvided_NoDiscount()
    {
        var result = _logic.CalculatePricing([Standard], [null], DateTime.UtcNow);

        Assert.AreEqual(15.00m, result.FinalPrice);
        Assert.AreEqual(0, result.Discounts.Count);
    }

    [TestMethod]
    public void Child_VipSeat_Gets50PercentOff()
    {
        var result = _logic.CalculatePricing([Vip], [8], DateTime.UtcNow);

        Assert.AreEqual(8.75m, result.FinalPrice);
    }

    [TestMethod]
    public void Senior_Age66_Standard_Gets20PercentOff()
    {
        var result = _logic.CalculatePricing([Standard], [66], DateTime.UtcNow);
        Assert.AreEqual(12.00m, result.FinalPrice);
        Assert.AreEqual("Senior Discount", result.SeatLines[0].Discount!.Name);
    }

    [TestMethod]
    public void Senior_Age65_Standard_NoDiscount()
    {
        var result = _logic.CalculatePricing([Standard], [65], DateTime.UtcNow);

        Assert.AreEqual(15.00m, result.FinalPrice);
        Assert.AreEqual(0, result.Discounts.Count);
    }

    [TestMethod]
    public void Group_SixStandardSeats_Gets20PercentOff()
    {
        var seats = Enumerable.Repeat(Standard, 6).ToList();
        var ages  = Enumerable.Repeat((int?)null, 6).ToList();

        var result = _logic.CalculatePricing(seats, ages, DateTime.UtcNow);

        Assert.AreEqual(90.00m, result.BasePrice);
        Assert.AreEqual(72.00m, result.FinalPrice);
        Assert.AreEqual("Group Discount", result.SeatLines[0].Discount!.Name);
    }

    [TestMethod]
    public void Group_FiveSeats_NoGroupDiscount()
    {
        var seats = Enumerable.Repeat(Standard, 5).ToList();
        var ages  = Enumerable.Repeat((int?)null, 5).ToList();

        var result = _logic.CalculatePricing(seats, ages, DateTime.UtcNow);

        Assert.AreEqual(75.00m, result.FinalPrice);
        Assert.AreEqual(0, result.Discounts.Count);
    }

    [TestMethod]
    public void GroupOf6_ChildSeat_ChildDiscountWins()
    {
        //best discount wins per seat
        var seats = Enumerable.Repeat(Standard, 6).ToList();
        var ages = new List<int?> { 8, null, null, null, null, null };

        var result = _logic.CalculatePricing(seats, ages, DateTime.UtcNow);

        // Other 5 seats: each gets Group 20% → 15.00 × 0.80 = 12.00
        Assert.AreEqual(7.50m,  result.SeatLines[0].FinalPrice);
        Assert.AreEqual("Child Discount",  result.SeatLines[0].Discount!.Name);
        Assert.AreEqual(12.00m, result.SeatLines[1].FinalPrice);
        Assert.AreEqual("Group Discount", result.SeatLines[1].Discount!.Name);
        Assert.AreEqual(7.50m + 5 * 12.00m, result.FinalPrice);
    }

    [TestMethod]
    public void MixedSeats_NoDiscount_SumsCorrectly()
    {
        var result = _logic.CalculatePricing([Standard, Vip, King], [30, 30, 30], DateTime.UtcNow);

        Assert.AreEqual(52.50m, result.BasePrice);
        Assert.AreEqual(52.50m, result.FinalPrice);
    }
}
