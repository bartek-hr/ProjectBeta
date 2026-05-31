using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class MovieScheduleAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private MovieScheduleAccess? _access;

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
        _context.Movies.Add(new Movie { Id = "m1", Title = "Film", Description = "d", RuntimeSeconds = 5400 });
        _context.SaveChanges();
        _access = new MovieScheduleAccess(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _connection?.Dispose();
    }

    [TestMethod]
    public void GetScheduleForDate_NoSchedules_ReturnsEmpty()
    {
        var result = _access!.GetScheduleForDate(DateOnly.FromDateTime(DateTime.Today));
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void AddSchedules_PersistsExpectedFields()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _access!.AddSchedules(new[]
        {
            new MovieSchedule
            {
                ScheduleDate = date,
                AuditoriumId = 1,
                MovieId = "m1",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 30)
            }
        });

        var saved = _context!.MovieSchedules.First();
        Assert.AreEqual(date, saved.ScheduleDate);
        Assert.AreEqual(1, saved.AuditoriumId);
        Assert.AreEqual("m1", saved.MovieId);
        Assert.AreEqual(new TimeOnly(9, 0), saved.StartTime);
        Assert.AreEqual(new TimeOnly(10, 30), saved.EndTime);
    }

    [TestMethod]
    public void GetScheduleForDate_ReturnsOnlyMatchingDate()
    {
        var today = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var tomorrow = today.AddDays(1);

        _access!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = today,    AuditoriumId = 1, MovieId = "m1", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) },
            new MovieSchedule { ScheduleDate = tomorrow, AuditoriumId = 2, MovieId = "m1", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _access.GetScheduleForDate(today);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(today, result[0].ScheduleDate);
    }

    [TestMethod]
    public void GetScheduleForDate_OrdersByAuditoriumThenStartTime()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        _context!.Movies.Add(new Movie { Id = "m2", Title = "Film B", Description = "d", RuntimeSeconds = 3600 });
        _context.SaveChanges();

        _access!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 2, MovieId = "m1", StartTime = new TimeOnly(9,  0), EndTime = new TimeOnly(10, 30) },
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m2", StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(13, 0)  },
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1", StartTime = new TimeOnly(9,  0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _access.GetScheduleForDate(date);
        Assert.AreEqual(1, result[0].AuditoriumId);
        Assert.AreEqual(new TimeOnly(9, 0), result[0].StartTime);
        Assert.AreEqual(1, result[1].AuditoriumId);
        Assert.AreEqual(new TimeOnly(12, 0), result[1].StartTime);
        Assert.AreEqual(2, result[2].AuditoriumId);
    }

    [TestMethod]
    public void GetScheduleForDate_IncludesMovieAndAuditorium()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        _access!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1",
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _access.GetScheduleForDate(date);
        Assert.IsNotNull(result[0].Movie);
        Assert.IsNotNull(result[0].Auditorium);
    }

    [TestMethod]
    public void HasScheduleForDate_NoSchedule_ReturnsFalse()
    {
        var result = _access!.HasScheduleForDate(DateOnly.FromDateTime(DateTime.Today.AddDays(99)));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasScheduleForDate_WithSchedule_ReturnsTrue()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(4));
        _access!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1",
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });
        Assert.IsTrue(_access.HasScheduleForDate(date));
    }
}
