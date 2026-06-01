using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Logic;

[TestClass]
public class LocationOpeningTimeLogicTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private LocationOpeningTimeLogic? _logic;
    private MovieScheduleAccess? _movieScheduleAccess;

    private User AdminUser => new User
    {
        Id = 99,
        Username = "admin_test",
        Role = "Admin",
        PasswordHash = "x",
        Email = "admin@example.com",
        FirstName = "Admin",
        LastName = "User",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    private User RegularUser => new User
    {
        Id = 98,
        Username = "regular_test",
        Role = "User",
        PasswordHash = "x",
        Email = "regular@example.com",
        FirstName = "Regular",
        LastName = "User",
        DateOfBirth = new DateOnly(1995, 1, 1)
    };

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

        _movieScheduleAccess = new MovieScheduleAccess(_context);
        _logic = new LocationOpeningTimeLogic(
            new LocationOpeningTimeAccess(_context),
            new LocationAccess(_context),
            _movieScheduleAccess);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetByLocationId_EnsuresPermanentDefaultOpeningTime()
    {
        var rules = _logic!.GetByLocationId(1);

        var defaultRule = rules.Single(rule =>
            rule.StartDate == DateOnly.MinValue
            && rule.ExpiresAt == DateOnly.MaxValue);

        Assert.AreEqual(new TimeOnly(9, 0), defaultRule.OpeningTime);
        Assert.AreEqual(new TimeOnly(20, 0), defaultRule.ClosingTime);
        Assert.AreEqual(DateTime.MinValue, defaultRule.CreatedAt);
    }

    [TestMethod]
    public void GetOpeningHoursForDate_UsesDefaultWhenNoOverrideExists()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        var hours = _logic!.GetOpeningHoursForDate(1, date);

        Assert.IsFalse(hours.IsClosed);
        Assert.AreEqual(new TimeOnly(9, 0), hours.OpeningTime);
        Assert.AreEqual(new TimeOnly(20, 0), hours.ClosingTime);
    }

    [TestMethod]
    public void GetOpeningHoursForDate_LatestCreatedMatchingRuleWins()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        _context!.LocationOpeningTimes.AddRange(
            new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date,
                ExpiresAt = date,
                OpeningTime = new TimeOnly(10, 0),
                ClosingTime = new TimeOnly(18, 0),
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date,
                ExpiresAt = date,
                OpeningTime = new TimeOnly(13, 0),
                ClosingTime = new TimeOnly(22, 0),
                CreatedAt = DateTime.UtcNow
            });
        _context.SaveChanges();

        var hours = _logic!.GetOpeningHoursForDate(1, date);

        Assert.AreEqual(new TimeOnly(13, 0), hours.OpeningTime);
        Assert.AreEqual(new TimeOnly(22, 0), hours.ClosingTime);
    }

    [TestMethod]
    public void GetOpeningHoursForDate_NullTimesMeanClosed()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        _context!.LocationOpeningTimes.Add(new LocationOpeningTime
        {
            LocationId = 1,
            StartDate = date,
            ExpiresAt = date,
            OpeningTime = null,
            ClosingTime = null,
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var hours = _logic!.GetOpeningHoursForDate(1, date);

        Assert.IsTrue(hours.IsClosed);
    }

    [TestMethod]
    public void GetByLocationId_HidesExpiredOpeningTimes()
    {
        var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        _context!.LocationOpeningTimes.Add(new LocationOpeningTime
        {
            LocationId = 1,
            StartDate = expiredDate,
            ExpiresAt = expiredDate,
            OpeningTime = new TimeOnly(11, 0),
            ClosingTime = new TimeOnly(12, 0),
            CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        var rules = _logic!.GetByLocationId(1);

        Assert.IsFalse(rules.Any(rule => rule.ExpiresAt == expiredDate));
    }

    [TestMethod]
    public void Add_AsRegularUser_ThrowsUnauthorized()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(4));

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _logic!.Add(new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date,
                ExpiresAt = date,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(17, 0)
            }, RegularUser));
    }

    [TestMethod]
    public void Add_StartDateAfterExpiresAt_ThrowsInvalidOperation()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic!.Add(new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date.AddDays(1),
                ExpiresAt = date,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(17, 0)
            }, AdminUser));
    }

    [TestMethod]
    public void Add_OnlyOneTimeSet_ThrowsInvalidOperation()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(6));

        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic!.Add(new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date,
                ExpiresAt = date,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = null
            }, AdminUser));
    }

    [TestMethod]
    public void Add_OpeningAtOrAfterClosing_ThrowsInvalidOperation()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        Assert.ThrowsException<InvalidOperationException>(() =>
            _logic!.Add(new LocationOpeningTime
            {
                LocationId = 1,
                StartDate = date,
                ExpiresAt = date,
                OpeningTime = new TimeOnly(17, 0),
                ClosingTime = new TimeOnly(9, 0)
            }, AdminUser));
    }

    [TestMethod]
    public void Add_InvalidatesOnlyMatchingLocationSchedulesInDateRange()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(8));
        _context!.Locations.Add(new Location { Id = 2, Name = "Other", City = "Utrecht", Address = "Other St 1" });
        _context.Auditoriums.Add(new Auditorium { Id = 4, Name = "Other Hall", LocationId = 2, Capacity = 100 });
        _context.Movies.Add(new Movie { Id = "m1", Title = "Movie", Description = "d", RuntimeSeconds = 5400 });
        _context.SaveChanges();

        _movieScheduleAccess!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) },
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 4, MovieId = "m1", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) },
            new MovieSchedule { ScheduleDate = date.AddDays(1), AuditoriumId = 1, MovieId = "m1", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        _logic!.Add(new LocationOpeningTime
        {
            LocationId = 1,
            StartDate = date,
            ExpiresAt = date,
            OpeningTime = new TimeOnly(10, 0),
            ClosingTime = new TimeOnly(18, 0)
        }, AdminUser);

        Assert.IsFalse(_context.MovieSchedules.Any(schedule => schedule.ScheduleDate == date && schedule.AuditoriumId == 1));
        Assert.IsTrue(_context.MovieSchedules.Any(schedule => schedule.ScheduleDate == date && schedule.AuditoriumId == 4));
        Assert.IsTrue(_context.MovieSchedules.Any(schedule => schedule.ScheduleDate == date.AddDays(1) && schedule.AuditoriumId == 1));
    }
}
