using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;

namespace ProjectBeta.Tests;

[TestClass]
public class SubscriptionLogicTests
{
    private AppDbContext? _context;
    private SubscriptionLogic? _logic;
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
        _logic = new SubscriptionLogic(new SubscriptionAccess(_context), new UserAccess(_context));
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void CreateSubscription_ValidSeatType_CreatesActiveSubscription()
    {
        var result = _logic!.CreateSubscription(2, "VIP");

        Assert.IsTrue(result.Success);
        Assert.IsNull(result.FieldErrors);
        Assert.IsNotNull(result.Subscription);
        Assert.AreEqual("VIP", result.Subscription!.FreeMondaySeatType);
        Assert.IsTrue(result.Subscription.IsActive);
        Assert.AreEqual(0m, result.Subscription.DiscountPercentage);
        Assert.AreEqual(1, _context!.Subscriptions.Count());
    }

    [TestMethod]
    public void CreateSubscription_ExistingActiveSubscription_ReturnsError()
    {
        var first = _logic!.CreateSubscription(2, "Red");
        var second = _logic.CreateSubscription(2, "Standard");

        Assert.IsTrue(first.Success);
        Assert.IsFalse(second.Success);
        Assert.IsNotNull(second.FieldErrors);
        Assert.IsTrue(second.FieldErrors!.ContainsKey("General"));
        Assert.AreEqual(1, _context!.Subscriptions.Count());
    }

    [TestMethod]
    public void CreateSubscription_WithPartner_LinksBothUsersAndAppliesDuoDiscount()
    {
        var result = _logic!.CreateSubscription(2, "Yellow", 1);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Subscription);
        Assert.AreEqual(2, result.Subscription!.UserId);
        Assert.AreEqual(1, result.Subscription.PartnerUserId);
        Assert.AreEqual(10m, result.Subscription.DiscountPercentage);
        Assert.AreEqual(18.00m, result.Subscription.CurrentMonthlyPrice);
        Assert.AreEqual(result.Subscription.Id, _logic.GetActiveSubscription(1)!.Id);
    }

    [TestMethod]
    public void CreateSubscription_PartnerAlreadyHasActiveSubscription_ReturnsError()
    {
        _logic!.CreateSubscription(1, "Standard");

        var result = _logic.CreateSubscription(2, "VIP", 1);

        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.FieldErrors);
        Assert.IsTrue(result.FieldErrors!.ContainsKey("Partner"));
    }

    [TestMethod]
    public void CancelSubscription_IndividualSubscription_MarksItInactive()
    {
        _logic!.CreateSubscription(2, "Red");

        var result = _logic.CancelSubscription(2);

        Assert.IsTrue(result.Success);
        var subscription = _context!.Subscriptions.Single();
        Assert.IsFalse(subscription.IsActive);
        Assert.IsNotNull(subscription.CancelledAt);
    }

    [TestMethod]
    public void CancelSubscription_SharedSubscription_RemovesCancellingUserAndDiscount()
    {
        _logic!.CreateSubscription(2, "Yellow", 1);

        var result = _logic.CancelSubscription(1);

        Assert.IsTrue(result.Success);
        var subscription = _context!.Subscriptions.Single();
        Assert.IsTrue(subscription.IsActive);
        Assert.IsNull(subscription.PartnerUserId);
        Assert.AreEqual(0m, subscription.DiscountPercentage);
        Assert.IsNull(_logic.GetActiveSubscription(1));
        Assert.IsNotNull(_logic.GetActiveSubscription(2));
    }

    [TestMethod]
    public void HasFreeMondayBenefit_MatchesOnlyMondayAndSeatType()
    {
        _logic!.CreateSubscription(2, "VIP");

        Assert.IsTrue(_logic.HasFreeMondayBenefit(2, "VIP", new DateOnly(2026, 5, 4)));
        Assert.IsFalse(_logic.HasFreeMondayBenefit(2, "Standard", new DateOnly(2026, 5, 4)));
        Assert.IsFalse(_logic.HasFreeMondayBenefit(2, "VIP", new DateOnly(2026, 5, 5)));
    }
}
