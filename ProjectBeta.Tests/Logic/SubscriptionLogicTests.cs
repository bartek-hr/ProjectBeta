using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Logic;

[TestClass]
public class SubscriptionLogicTests
{
    private AppDbContext _context = null!;
    private SqliteConnection _connection = null!;
    private SubscriptionLogic _logic = null!;

    // IDs matching seed data
    private const int AdminId = 1;
    private const int User1Id = 2;
    private const int User2Id = 3;

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

        _logic = new SubscriptionLogic(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    // GetAvailableSubscriptions

    [TestMethod]
    public void GetAvailableSubscriptions_ReturnsSeededSubscription()
    {
        var result = _logic.GetAvailableSubscriptions();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Free Monday", result[0].Name);
    }

    [TestMethod]
    public void GetAvailableSubscriptionsWithSeatPrice_IncludesSeatPrice()
    {
        var result = _logic.GetAvailableSubscriptionsWithSeatPrice();

        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].SeatPrice);
        Assert.AreEqual("Standard", result[0].SeatPrice.Name);
    }

    // UserHasActiveSubscription
    [TestMethod]
    public void UserHasActiveSubscription_WhenUserHasOne_ReturnsTrue()
    {
        Assert.IsTrue(_logic.UserHasActiveSubscription(User2Id));
    }

    [TestMethod]
    public void UserHasActiveSubscription_WhenUserHasNone_ReturnsFalse()
    {
        Assert.IsFalse(_logic.UserHasActiveSubscription(AdminId));
    }

    // BuySubscription

    [TestMethod]
    public void BuySubscription_CreatesActiveUserSubscription()
    {
        var sub = _context.Subscriptions.First();

        _logic.BuySubscription(User1Id, sub.Id);

        Assert.IsTrue(_logic.UserHasActiveSubscription(User1Id));
    }

    [TestMethod]
    public void BuySubscription_WhenAlreadyHasSubscription_Throws()
    {
        var sub = _context.Subscriptions.First();

        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic.BuySubscription(User2Id, sub.Id));
    }

    [TestMethod]
    public void BuySubscription_WhenSubscriptionNotFound_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic.BuySubscription(User1Id, 9999));
    }

    // CancelSubscription

    [TestMethod]
    public void CancelSubscription_DeactivatesUserSubscription()
    {
        _logic.CancelSubscription(User2Id);

        Assert.IsFalse(_logic.UserHasActiveSubscription(User2Id));
    }

    [TestMethod]
    public void CancelSubscription_SetsEndDate()
    {
        var before = DateTime.Now;

        _logic.CancelSubscription(User2Id);

        var userSub = _context.UserSubscriptions
            .First(us => us.UserId == User2Id);
        Assert.IsNotNull(userSub.EndDate);
        Assert.IsTrue(userSub.EndDate >= before);
    }

    [TestMethod]
    public void CancelSubscription_WhenConnected_DisconnectsPartner()
    {
        // Buy the same subscription for user1 then connect them
        var sub = _context.Subscriptions.First();
        _logic.BuySubscription(User1Id, sub.Id);
        _logic.ConnectSubscription(User2Id, "user1@example.com");

        // Cancel user2's subscription
        _logic.CancelSubscription(User2Id);

        // User1's subscription should be disconnected
        var user1Sub = _context.UserSubscriptions
            .First(us => us.UserId == User1Id && us.IsActive);
        Assert.IsFalse(user1Sub.IsConnected);
        Assert.IsNull(user1Sub.ConnectedWithUserId);
    }

    // ConnectSubscription

    [TestMethod]
    public void ConnectSubscription_WhenBothHaveSameSub_ConnectsBoth()
    {
        var sub = _context.Subscriptions.First();
        _logic.BuySubscription(User1Id, sub.Id);

        _logic.ConnectSubscription(User2Id, "user1@example.com");

        var user2Sub = _context.UserSubscriptions.First(us => us.UserId == User2Id && us.IsActive);
        var user1Sub = _context.UserSubscriptions.First(us => us.UserId == User1Id && us.IsActive);

        Assert.IsTrue(user2Sub.IsConnected);
        Assert.AreEqual(User1Id, user2Sub.ConnectedWithUserId);
        Assert.IsTrue(user1Sub.IsConnected);
        Assert.AreEqual(User2Id, user1Sub.ConnectedWithUserId);
    }

    [TestMethod]
    public void ConnectSubscription_EmptyEmail_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic.ConnectSubscription(User2Id, ""));
    }

    [TestMethod]
    public void ConnectSubscription_UnknownEmail_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic.ConnectSubscription(User2Id, "nobody@nowhere.com"));
    }

    [TestMethod]
    public void ConnectSubscription_WhenPartnerHasNoSubscription_Throws()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic.ConnectSubscription(User2Id, "user1@example.com"));
    }
    // CheckFriendSubscription

    [TestMethod]
    public void CheckFriendSubscription_UnknownEmail_ReturnsNotFound()
    {
        var sub = _context.Subscriptions.First();

        var result = _logic.CheckFriendSubscription(sub.Id, "nobody@nowhere.com");

        Assert.AreEqual(SubscriptionLogic.FriendCheckResult.NotFound, result);
    }

    [TestMethod]
    public void CheckFriendSubscription_FriendHasSubscription_ReturnsHasSubscription()
    {
        var sub = _context.Subscriptions.First();

        // User2 already has the seeded subscription
        var result = _logic.CheckFriendSubscription(sub.Id, "user2@example.com");

        Assert.AreEqual(SubscriptionLogic.FriendCheckResult.HasSubscription, result);
    }
    // GetActiveSubscriptionPricingInfo

    [TestMethod]
    public void GetActiveSubscriptionPricingInfo_WhenUserHasSubscription_ReturnsPricingContext()
    {
        var result = _logic.GetActiveSubscriptionPricingInfo(User2Id);

        Assert.IsNotNull(result);
        Assert.AreEqual((int)DayOfWeek.Monday, result.ApplicableDayOfWeek);
        Assert.AreEqual(15.00m, result.SubscriptionSeatPrice);
    }

    // AddSubscription / RemoveSubscription

    [TestMethod]
    public void AddSubscription_PersistsToDatabase()
    {
        var sub = new Subscription
        {
            Name = "Weekend Pass",
            Price = 30m,
            SeatPriceId = 1,
            IsActive = true,
            EffectiveFrom = DateTime.UtcNow
        };

        _logic.AddSubscription(sub);

        Assert.AreEqual(2, _context.Subscriptions.Count());
        Assert.IsTrue(_context.Subscriptions.Any(s => s.Name == "Weekend Pass"));
    }

    [TestMethod]
    public void RemoveSubscription_DeactivatesSubscription()
    {
        var sub = _context.Subscriptions.First();

        _logic.RemoveSubscription(sub.Id);

        Assert.IsFalse(_context.Subscriptions.First(s => s.Id == sub.Id).IsActive);
        Assert.AreEqual(0, _logic.GetAvailableSubscriptions().Count);
    }
}
