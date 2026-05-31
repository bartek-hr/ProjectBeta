using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.Tests.Helpers;

namespace ProjectBeta.Tests;

[TestClass]
public class BookingSnackLogicTests
{
    private static User Admin => new() { Id = 1, Username = "admin", Role = "Admin", Email = "a@a.com", FirstName = "A", LastName = "A", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };
    private static User SuperAdmin => new() { Id = 2, Username = "superadmin", Role = "SuperAdmin", Email = "b@b.com", FirstName = "B", LastName = "B", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };
    private static User RegularUser => new() { Id = 3, Username = "user", Role = "User", Email = "c@c.com", FirstName = "C", LastName = "C", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };

    private static (BookingSnackLogic logic, AppDbContext context) CreateLogic()
    {
        var context = TestDbContext.Create();
        var snackAccess = new SnackAccess(context);
        var bookingSnackAccess = new BookingSnackAccess(context);
        return (new BookingSnackLogic(bookingSnackAccess, snackAccess), context);
    }

    private static Booking SeedBooking(AppDbContext context)
    {
        var user = new User { Username = "u", Role = "User", Email = "u@u.com", FirstName = "U", LastName = "U", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };
        context.Users.Add(user);
        context.SaveChanges();
        var booking = new Booking { UserId = user.Id, ScreeningId = 1, TotalPrice = 10m, Paid = false, CreatedAt = DateTime.UtcNow, Seats = "" };
        context.Bookings.Add(booking);
        context.SaveChanges();
        return booking;
    }

    // --- GetAll ---

    [TestMethod]
    public void GetAll_ReturnsAllBookingSnacks()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();

        var result = logic.GetAll();

        Assert.AreEqual(1, result.Count);
    }

    // --- GetById ---

    [TestMethod]
    public void GetById_ReturnsCorrectBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var id = context.BookingSnacks.First().Id;

        var result = logic.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.BookedQuantity);
    }

    [TestMethod]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        var (logic, _) = CreateLogic();

        var result = logic.GetById(999);

        Assert.IsNull(result);
    }

    // --- Add ---

    [TestMethod]
    public void Add_AdminUser_AddsBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        var bookingSnack = new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow };

        logic.Add(bookingSnack, Admin);

        Assert.AreEqual(1, context.BookingSnacks.Count());
    }

    [TestMethod]
    public void Add_SuperAdminUser_AddsBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Cola", Price = 2.00m, Quantity = 50 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        var bookingSnack = new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 1, BookedAt = DateTime.UtcNow };

        logic.Add(bookingSnack, SuperAdmin);

        Assert.AreEqual(1, context.BookingSnacks.Count());
    }

    [TestMethod]
    public void Add_RegularUser_AddsBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        var bookingSnack = new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow };

        logic.Add(bookingSnack, RegularUser);

        Assert.AreEqual(1, context.BookingSnacks.Count());
    }

    [TestMethod]
    public void Add_ZeroQuantity_ThrowsArgumentException()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        var bookingSnack = new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 0, BookedAt = DateTime.UtcNow };

        Assert.ThrowsException<ArgumentException>(() => logic.Add(bookingSnack, Admin));
    }

    [TestMethod]
    public void Add_NonExistentSnack_ThrowsException()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var bookingSnack = new BookingSnack { SnackId = 999, BookingId = booking.Id, BookedQuantity = 1, BookedAt = DateTime.UtcNow };

        Assert.ThrowsException<Exception>(() => logic.Add(bookingSnack, Admin));
    }

    // --- Update ---

    [TestMethod]
    public void Update_AdminUser_UpdatesBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var bs = context.BookingSnacks.First();
        bs.BookedQuantity = 5;

        logic.Update(bs, Admin);

        Assert.AreEqual(5, context.BookingSnacks.First().BookedQuantity);
    }

    [TestMethod]
    public void Update_RegularUser_ThrowsUnauthorized()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var bs = context.BookingSnacks.First();

        Assert.ThrowsException<UnauthorizedAccessException>(() => logic.Update(bs, RegularUser));
    }

    // --- Delete ---

    [TestMethod]
    public void Delete_AdminUser_DeletesBookingSnack()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var id = context.BookingSnacks.First().Id;

        logic.Delete(id, Admin);

        Assert.AreEqual(0, context.BookingSnacks.Count());
    }

    [TestMethod]
    public void Delete_RegularUser_ThrowsUnauthorized()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var id = context.BookingSnacks.First().Id;

        Assert.ThrowsException<UnauthorizedAccessException>(() => logic.Delete(id, RegularUser));
    }

    [TestMethod]
    public void Delete_NonExistentBookingSnack_ThrowsException()
    {
        var (logic, _) = CreateLogic();

        Assert.ThrowsException<Exception>(() => logic.Delete(999, Admin));
    }

    [TestMethod]
    public void GetAllByBookingId_ReturnsOnlyMatchingBookingSnacks()
    {
        var (logic, context) = CreateLogic();
        var booking1 = SeedBooking(context);
        var booking2 = SeedBooking(context);
        var snack = new Snack { Name = "Chips", Price = 2.50m, Quantity = 50 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.AddRange(
            new BookingSnack { SnackId = snack.Id, BookingId = booking1.Id, BookedQuantity = 1, BookedAt = DateTime.UtcNow },
            new BookingSnack { SnackId = snack.Id, BookingId = booking2.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow }
        );
        context.SaveChanges();

        var result = logic.GetAllByBookingId(booking1.Id);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(booking1.Id, result[0].BookingId);
    }

    [TestMethod]
    public void GetAllByBookingId_NoSnacks_ReturnsEmpty()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);

        var result = logic.GetAllByBookingId(booking.Id);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Update_WithZeroQuantity_ThrowsArgumentException()
    {
        var (logic, context) = CreateLogic();
        var booking = SeedBooking(context);
        var snack = new Snack { Name = "Nachos", Price = 4m, Quantity = 100 };
        context.Snacks.Add(snack);
        context.SaveChanges();
        context.BookingSnacks.Add(new BookingSnack { SnackId = snack.Id, BookingId = booking.Id, BookedQuantity = 2, BookedAt = DateTime.UtcNow });
        context.SaveChanges();
        var bs = context.BookingSnacks.First();
        bs.BookedQuantity = 0;

        Assert.ThrowsException<ArgumentException>(() => logic.Update(bs, Admin));
    }
}
