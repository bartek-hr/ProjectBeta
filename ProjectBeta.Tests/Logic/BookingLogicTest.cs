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
            Total_Price = 20,
            User_Id = 1,
            Screening_ID = 1
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
            Total_Price = 0,
            User_Id = 1,
            Screening_ID = 1
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
            Total_Price = 10,
            User_Id = 1,
            Screening_ID = 1
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
            Total_Price = 15,
            User_Id = 1,
            Screening_ID = 1,
            Paid = false
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
            Total_Price = 15,
            User_Id = 1,
            Screening_ID = 1
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
            new Booking { Total_Price = 10, User_Id = 1, Screening_ID = 1 },
            new Booking { Total_Price = 20, User_Id = 2, Screening_ID = 2 }
        );
        context.SaveChanges();

        var result = logic.GetBookings();

        Assert.AreEqual(2, result.Count);
    }
}