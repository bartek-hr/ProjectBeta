using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Model;

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
    public void EnsureCreated_SeedsDefaultLocation()
    {
        Assert.AreEqual(1, _context!.Locations.Count());
        Assert.AreEqual("Main Location", _context.Locations.First().Name);
        Assert.AreEqual("Rotterdam", _context.Locations.First().City);
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

    [TestMethod]
    public void MovieSchedules_UniqueIndex_RejectsDuplicateDateAuditoriumAndStartTime()
    {
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Movie", Description = "Desc", RuntimeSeconds = 5400 });
        _context.SaveChanges();

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _context.MovieSchedules.Add(new MovieSchedule
        {
            ScheduleDate = date,
            AuditoriumId = 1,
            MovieId = "m1",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30)
        });
        _context.SaveChanges();

        _context.MovieSchedules.Add(new MovieSchedule
        {
            ScheduleDate = date,
            AuditoriumId = 1,
            MovieId = "m1",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 45)
        });

        Assert.ThrowsException<DbUpdateException>(() => _context.SaveChanges());
    }

    [TestMethod]
    public void DeletingMovie_CascadesToRelatedSchedules()
    {
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Movie", Description = "Desc", RuntimeSeconds = 5400 });
        _context.SaveChanges();

        _context.MovieSchedules.Add(new MovieSchedule
        {
            ScheduleDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            AuditoriumId = 1,
            MovieId = "m1",
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30)
        });
        _context.SaveChanges();

        var movie = _context.Movies.Find("m1")!;
        _context.Movies.Remove(movie);
        _context.SaveChanges();

        Assert.AreEqual(0, _context.MovieSchedules.Count());
    }

    [TestMethod]
    public void DeletingLocation_CascadesToAuditoriums()
    {
        var location = new Location { Name = "Downtown", City = "Rotterdam", Address = "St 1" };
        _context!.Locations.Add(location);
        _context.SaveChanges();

        _context.Auditoriums.Add(new Auditorium { Name = "Aud", Capacity = 100, LocationId = location.Id });
        _context.SaveChanges();
        int beforeCount = _context.Auditoriums.Count();

        _context.Locations.Remove(location);
        _context.SaveChanges();

        Assert.AreEqual(beforeCount - 1, _context.Auditoriums.Count());
    }
}
