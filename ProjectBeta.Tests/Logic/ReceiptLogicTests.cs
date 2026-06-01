using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests;

[TestClass]
public class ReceiptLogicTests
{
    private AppDbContext? _context;
    private ReceiptAccess? _receiptAccess;
    private ReceiptLogic? _logic;
    private BookingAccess? _bookingAccess;
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
        _receiptAccess = new ReceiptAccess(_context);
        _bookingAccess = new BookingAccess(_context);
        _logic = new ReceiptLogic(_receiptAccess, _bookingAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    private int SeedBooking()
    {
        var booking = new Booking
        {
            UserId = 1, ScreeningId = 1, Seats = "A1",
            BasePrice = 15m, TotalPrice = 15m, Paid = false,
            AuditoriumId = 1
        };
        _context!.Bookings.Add(booking);
        _context.SaveChanges();
        return booking.Id;
    }

    [TestMethod]
    public void GetReceipts_ReturnsAll()
    {
        var bookingId = SeedBooking();
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 15m });
        var result = _logic.GetReceipts();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetReceipt_ExistingId_ReturnsReceipt()
    {
        var bookingId = SeedBooking();
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 15m });
        var id = _context!.Receipts.First().Id;
        var result = _logic.GetReceipt(id);
        Assert.IsNotNull(result);
        Assert.AreEqual(id, result.Id);
    }

    [TestMethod]
    public void GetReceipt_NotFound_ThrowsException()
    {
        Assert.ThrowsException<Exception>(() => _logic!.GetReceipt(999));
    }

    [TestMethod]
    public void CreateReceipt_ValidReceipt_SavesSuccessfully()
    {
        var bookingId = SeedBooking();
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 20m });
        Assert.AreEqual(1, _context!.Receipts.Count());
    }

    [TestMethod]
    public void CreateReceipt_SetsCreatedAt()
    {
        var bookingId = SeedBooking();
        var before = DateTime.Now.AddSeconds(-1);
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 20m });
        var saved = _context!.Receipts.First();
        Assert.IsTrue(saved.CreatedAt >= before);
    }

    [TestMethod]
    public void CreateReceipt_ZeroTotal_ThrowsException()
    {
        var bookingId = SeedBooking();
        Assert.ThrowsException<Exception>(() =>
            _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 0m }));
    }

    [TestMethod]
    public void CreateReceipt_NegativeTotal_ThrowsException()
    {
        var bookingId = SeedBooking();
        Assert.ThrowsException<Exception>(() =>
            _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = -5m }));
    }

    [TestMethod]
    public void CreateReceipt_InvalidBookingId_ThrowsException()
    {
        Assert.ThrowsException<Exception>(() =>
            _logic!.CreateReceipt(new Receipt { BookingId = 0, Total = 15m }));
    }

    [TestMethod]
    public void CreateReceipt_NonexistentBooking_ThrowsException()
    {
        Assert.ThrowsException<Exception>(() =>
            _logic!.CreateReceipt(new Receipt { BookingId = 999, Total = 15m }));
    }

    [TestMethod]
    public void MarkAsPaid_ExistingReceipt_MarksBookingAsPaid()
    {
        var bookingId = SeedBooking();
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 15m });
        var id = _context!.Receipts.First().Id;
        _logic.MarkAsPaid(id);
        var booking = _context.Bookings.Find(bookingId);
        Assert.IsTrue(booking!.Paid);
    }

    [TestMethod]
    public void MarkAsPaid_NotFound_ThrowsException()
    {
        Assert.ThrowsException<Exception>(() => _logic!.MarkAsPaid(999));
    }

    [TestMethod]
    public void DeleteReceipt_ExistingId_RemovesReceipt()
    {
        var bookingId = SeedBooking();
        _logic!.CreateReceipt(new Receipt { BookingId = bookingId, Total = 15m });
        var id = _context!.Receipts.First().Id;
        _logic.DeleteReceipt(id);
        Assert.AreEqual(0, _context.Receipts.Count());
    }

    [TestMethod]
    public void DeleteReceipt_NonExistingId_DoesNotThrow()
    {
        _logic!.DeleteReceipt(999);
    }
}
