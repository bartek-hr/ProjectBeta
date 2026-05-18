using System.Net;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.Tests.Helpers;

namespace ProjectBeta.Tests;

[TestClass]
public class MovieLogicTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;
    private MovieLogic? _logic;
    private MovieAccess? _movieAccess;
    private MovieScheduleAccess? _movieScheduleAccess;
    private AuditoriumLogic? _auditoriumLogic;

    private static readonly string TrendingResponse = """
        {
            "titles": [
                {"id": "m1", "type": "movie", "primaryTitle": "Movie One",   "plot": "p1", "genres": ["Action"], "runtimeSeconds": 5400},
                {"id": "m2", "type": "movie", "primaryTitle": "Movie Two",   "plot": "p2", "genres": ["Drama"],  "runtimeSeconds": 6000},
                {"id": "m3", "type": "movie", "primaryTitle": "Movie Three", "plot": "p3", "genres": [],         "runtimeSeconds": 4800}
            ]
        }
        """;

    private static readonly string EmptyTrendingResponse = """{"titles": []}""";

    [TestInitialize]
    public void Setup()
    {
        ResetTrendingCache();

        _connection = new SqliteConnection("DataSource=:memory:");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _connection.Open();
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var httpClient = new HttpClient(new FakeHttpMessageHandler(TrendingResponse))
        {
            BaseAddress = new Uri("https://fake.imdb.local/")
        };

        _movieAccess = new MovieAccess(_context, httpClient);
        _movieScheduleAccess = new MovieScheduleAccess(_context);
        var auditoriumAccess = new AuditoriumAccess(_context);
        _auditoriumLogic = new AuditoriumLogic(auditoriumAccess);
        _logic = new MovieLogic(_movieAccess, _movieScheduleAccess, _auditoriumLogic);
    }

    [TestCleanup]
    public void Cleanup()
    {
        ResetTrendingCache();
        _context?.Dispose();
        _connection?.Dispose();
    }

    private static void ResetTrendingCache()
    {
        var field = typeof(MovieAccess).GetField("_trendingCache", BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, null);
    }

    [TestMethod]
    public void GetScheduleForDate_ReturnsSchedules()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Movie One", Description = "p1", RuntimeSeconds = 5400 });
        _context.SaveChanges();
        _movieScheduleAccess!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1",
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _logic!.GetScheduleForDate(date);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetOrGenerateSchedule_ExistingSchedule_ReturnsCachedResult()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Movie One", Description = "p1", RuntimeSeconds = 5400 });
        _context.SaveChanges();
        _movieScheduleAccess!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1",
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _logic!.GetOrGenerateSchedule(date);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetOrGenerateSchedule_PastDate_ReturnsEmpty()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var result = _logic!.GetOrGenerateSchedule(pastDate);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetOrGenerateSchedule_FutureDate_GeneratesSchedule()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var result = _logic!.GetOrGenerateSchedule(date);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public void GenerateSchedule_ExistingSchedule_ReturnsExisting()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Movie One", Description = "p1", RuntimeSeconds = 5400 });
        _context.SaveChanges();
        _movieScheduleAccess!.AddSchedules(new[]
        {
            new MovieSchedule { ScheduleDate = date, AuditoriumId = 1, MovieId = "m1",
                StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 30) }
        });

        var result = _logic!.GenerateSchedule(date);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GenerateSchedule_NoAuditoriums_ReturnsEmpty()
    {
        _context!.Auditoriums.RemoveRange(_context.Auditoriums);
        _context.SaveChanges();

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(4));
        var result = _logic!.GenerateSchedule(date);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GenerateSchedule_NoValidMovies_ThrowsInvalidOperation()
    {
        ResetTrendingCache();
        var emptyClient = new HttpClient(new FakeHttpMessageHandler(EmptyTrendingResponse))
        {
            BaseAddress = new Uri("https://fake.imdb.local/")
        };
        var emptyAccess = new MovieAccess(_context!, emptyClient);
        var logic = new MovieLogic(emptyAccess, _movieScheduleAccess!, _auditoriumLogic!);

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        Assert.ThrowsException<InvalidOperationException>(() => logic.GenerateSchedule(date));
    }

    [TestMethod]
    public void GenerateSchedule_FiltersMoviesWithoutRuntime()
    {
        ResetTrendingCache();
        var response = """
            {
                "titles": [
                    {"id": "bad1", "type": "movie", "primaryTitle": "No Runtime", "plot": "x", "genres": [], "runtimeSeconds": null},
                    {"id": "m1",   "type": "movie", "primaryTitle": "Valid",      "plot": "y", "genres": [], "runtimeSeconds": 5400}
                ]
            }
            """;
        var client = new HttpClient(new FakeHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://fake.imdb.local/")
        };
        var access = new MovieAccess(_context!, client);
        var logic = new MovieLogic(access, _movieScheduleAccess!, _auditoriumLogic!);

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(6));
        var result = logic.GenerateSchedule(date);
        Assert.IsTrue(result.All(s => s.MovieId == "m1"));
    }

    [TestMethod]
    public void GenerateSchedule_DeduplicatesMovieIds()
    {
        ResetTrendingCache();
        var response = """
            {
                "titles": [
                    {"id": "dup", "type": "movie", "primaryTitle": "Dupe A", "plot": "x", "genres": [], "runtimeSeconds": 5400},
                    {"id": "dup", "type": "movie", "primaryTitle": "Dupe B", "plot": "y", "genres": [], "runtimeSeconds": 5400}
                ]
            }
            """;
        var client = new HttpClient(new FakeHttpMessageHandler(response))
        {
            BaseAddress = new Uri("https://fake.imdb.local/")
        };
        var access = new MovieAccess(_context!, client);
        var logic = new MovieLogic(access, _movieScheduleAccess!, _auditoriumLogic!);

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var result = logic.GenerateSchedule(date);
        Assert.IsTrue(result.Count > 0);
        Assert.AreEqual(1, _context!.Movies.Count(movie => movie.Id == "dup"));
    }

    [TestMethod]
    public void GenerateSchedule_PersistsMoviesAndSchedules()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(8));
        var result = _logic!.GenerateSchedule(date);
        Assert.IsTrue(result.Count > 0);
        Assert.IsTrue(_context!.Movies.Any());
        Assert.IsTrue(_context.MovieSchedules.Any(s => s.ScheduleDate == date));
    }

    [TestMethod]
    public void GenerateSchedule_RespectsOpeningClosingAndBreakConstraints()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(9));
        var result = _logic!.GenerateSchedule(date);

        Assert.IsTrue(result.Count > 0);

        foreach (var auditoriumSchedules in result.GroupBy(schedule => schedule.AuditoriumId))
        {
            var ordered = auditoriumSchedules.OrderBy(schedule => schedule.StartTime).ToList();
            Assert.IsTrue(ordered.All(schedule => schedule.StartTime >= TimeOnly.ParseExact(MovieLogic.OpeningTime, "HH:mm")));
            Assert.IsTrue(ordered.All(schedule => schedule.EndTime <= TimeOnly.ParseExact(MovieLogic.ClosingTime, "HH:mm")));

            for (var i = 1; i < ordered.Count; i++)
            {
                var breakMinutes = (ordered[i].StartTime - ordered[i - 1].EndTime).TotalMinutes;
                Assert.IsTrue(
                    breakMinutes >= MovieLogic.MinimumBreakMinutes,
                    $"Expected at least {MovieLogic.MinimumBreakMinutes} minutes between screenings, but got {breakMinutes}.");
            }
        }
    }

    [TestMethod]
    public void Search_BlankQuery_ReturnsEmpty()
    {
        var result = _logic!.Search("   ");
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void Search_MatchingQuery_ReturnsMovies()
    {
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Inception", Description = "x", RuntimeSeconds = 5400 });
        _context.SaveChanges();

        var result = _logic!.Search("Incep");
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void SearchSchedule_BlankQuery_ReturnsAll()
    {
        var schedules = new List<MovieSchedule>
        {
            new() { Movie = new Movie { Id = "m1", Title = "A", Description = "x" }, AuditoriumId = 1,
                    ScheduleDate = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(10,0) }
        };
        var result = _logic!.SearchSchedule(schedules, "  ");
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void SearchSchedule_MatchingQuery_FiltersResults()
    {
        var schedules = new List<MovieSchedule>
        {
            new() { Movie = new Movie { Id = "m1", Title = "Inception", Description = "x" }, AuditoriumId = 1,
                    ScheduleDate = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(10,0) },
            new() { Movie = new Movie { Id = "m2", Title = "Avatar", Description = "y" }, AuditoriumId = 1,
                    ScheduleDate = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(11,0), EndTime = new TimeOnly(12,0) }
        };
        var result = _logic!.SearchSchedule(schedules, "Incep");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Inception", result[0].Movie.Title);
    }
}
