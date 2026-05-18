using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Access;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests;

[TestClass]
public class BookingLogicTest
{
    private AppDbContext? _context;
    private BookingLogic? _logic;
    private SqliteConnection? _connection;

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

        var bookingAccess = new BookingAccess(_context);
        _logic = new BookingLogic(bookingAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void CreateBooking_ValidBooking_SavesToDatabase()
    {
        var logic = _logic!;
        var context = _context!;

        var booking = new Booking
        {
            TotalPrice = 20,
            UserId = 1,
            AuditoriumId = 1,
            Seats = "A1"
        };

        logic.CreateBooking(booking);

        Assert.AreEqual(1, context.Bookings.Count());
        Assert.IsFalse(booking.Paid);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void CreateBooking_InvalidPrice_ThrowsException()
    {
        var logic = _logic!;

        var booking = new Booking
        {
            TotalPrice = 0,
            UserId = 1,
            AuditoriumId = 1
        };

        logic.CreateBooking(booking);
    }

    [TestMethod]
    public void GetBooking_ExistingBooking_ReturnsBooking()
    {
        var context = _context!;
        var logic = _logic!;

        var booking = new Booking
        {
            TotalPrice = 10,
            UserId = 1,
            AuditoriumId = 1,
            Seats = ""
        };

        context.Bookings.Add(booking);
        context.SaveChanges();

        var result = logic.GetBooking(booking.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(booking.Id, result.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void GetBooking_NotFound_ThrowsException()
    {
        var logic = _logic!;

        logic.GetBooking(999);
    }

    [TestMethod]
    public void MarkAsPaid_ValidBooking_UpdatesPaidStatus()
    {
        var context = _context!;
        var logic = _logic!;

        var booking = new Booking
        {
            TotalPrice = 15,
            UserId = 1,
            AuditoriumId = 1,
            Paid = false,
            Seats = ""
        };

        context.Bookings.Add(booking);
        context.SaveChanges();

        logic.MarkAsPaid(booking.Id);

        var updated = context.Bookings.First();
        Assert.IsTrue(updated.Paid);
    }

    [TestMethod]
    public void DeleteBooking_RemovesBooking()
    {
        var context = _context!;
        var logic = _logic!;

        var booking = new Booking
        {
            TotalPrice = 15,
            UserId = 1,
            AuditoriumId = 1,
            Seats = ""
        };

        context.Bookings.Add(booking);
        context.SaveChanges();

        logic.DeleteBooking(booking.Id);

        Assert.AreEqual(0, context.Bookings.Count());
    }

    [TestMethod]
    public void GetBookings_ReturnsAllBookings()
    {
        var context = _context!;
        var logic = _logic!;

        context.Bookings.AddRange(
            new Booking { TotalPrice = 10, UserId = 1, AuditoriumId = 1, Seats = "" },
            new Booking { TotalPrice = 20, UserId = 2, AuditoriumId = 2, Seats = "" }
        );
        context.SaveChanges();

        var result = logic.GetBookings();

        Assert.AreEqual(2, result.Count);
    }

    // --- Overload with discounts ---

    [TestMethod]
    public void CreateBooking_Overload_SavesWithDiscounts()
    {
        _context!.Seed();
        var discount = _context.Discounts.First();

        var result = _logic!.CreateBooking(
            userId: 1, finalPrice: 12.00m, basePrice: 15.00m,
            auditoriumId: 1, seats: "A1,A2", seatAges: "30,12",
            userSeat: null, movie: "Inception",
            createdAt: DateTime.Now,
            appliedDiscountIds: new[] { discount.Id }
        );

        Assert.IsNotNull(result);
        Assert.AreEqual(1, _context.Bookings.Count());
        Assert.IsFalse(result.Paid);
        Assert.AreEqual(1, result.BookingDiscounts.Count);
    }

    [TestMethod]
    public void CreateBooking_Overload_DeduplicatesDiscountIds()
    {
        _context!.Seed();
        var discount = _context.Discounts.First();

        var result = _logic!.CreateBooking(
            userId: 1, finalPrice: 12.00m, basePrice: 15.00m,
            auditoriumId: 1, seats: "A1", seatAges: "30",
            userSeat: null, movie: "Film",
            createdAt: DateTime.Now,
            appliedDiscountIds: new[] { discount.Id, discount.Id }
        );

        Assert.AreEqual(1, result.BookingDiscounts.Count);
    }

    [TestMethod]
    public void CreateBooking_Overload_DefaultsPaidToFalse()
    {
        var result = _logic!.CreateBooking(
            userId: 1, finalPrice: 20m, basePrice: 20m,
            auditoriumId: 1, seats: "B1", seatAges: "25",
            userSeat: null, movie: "Film",
            createdAt: DateTime.Now,
            appliedDiscountIds: Array.Empty<int>()
        );

        Assert.IsFalse(result.Paid);
    }

    // --- MarkAsPaid not-found ---

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void MarkAsPaid_NotFound_ThrowsException()
    {
        _logic!.MarkAsPaid(9999);
    }

    // --- Filter methods ---

    [TestMethod]
    public void GetBookingsByUserId_ReturnsOnlyMatchingUser()
    {
        _context!.Bookings.AddRange(
            new Booking { UserId = 1, AuditoriumId = 1, TotalPrice = 10, Seats = "" },
            new Booking { UserId = 2, AuditoriumId = 1, TotalPrice = 20, Seats = "" }
        );
        _context.SaveChanges();

        var result = _logic!.GetBookingsByUserId(1);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result.All(b => b.UserId == 1));
    }

    [TestMethod]
    public void GetBookingsByUserId_NoMatch_ReturnsEmpty()
    {
        var result = _logic!.GetBookingsByUserId(999);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetBookingsByCreatedAtAndAuditoriumID_ReturnsMatchingBookings()
    {
        var createdAt = new DateTime(2026, 1, 1, 12, 0, 0);
        _context!.Bookings.AddRange(
            new Booking { UserId = 1, AuditoriumId = 1, TotalPrice = 10, Seats = "", CreatedAt = createdAt },
            new Booking { UserId = 1, AuditoriumId = 2, TotalPrice = 10, Seats = "", CreatedAt = createdAt }
        );
        _context.SaveChanges();

        var result = _logic!.GetBookingsByCreatedAtAndAuditoriumID(createdAt, 1);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].AuditoriumId);
    }
}