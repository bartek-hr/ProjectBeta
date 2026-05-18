using System.Net;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Model;
using ProjectBeta.Tests.Helpers;

namespace ProjectBeta.Tests.Access;

[TestClass]
public class MovieAccessTests
{
    private AppDbContext? _context;
    private SqliteConnection? _connection;

    private static readonly string ValidTrendingJson = """
        {
            "titles": [
                {"id": "m1", "type": "movie", "primaryTitle": "Trending Film", "plot": "desc", "genres": ["Action"], "runtimeSeconds": 5400, "rating": {"aggregateRating": 7.5}},
                {"id": "m2", "type": "movie", "primaryTitle": "Another Film",  "plot": "desc", "genres": [],         "runtimeSeconds": 3600},
                {"id": "tv1","type": "tvSeries","primaryTitle": "TV Show",     "plot": "desc", "genres": [],         "runtimeSeconds": 1800}
            ]
        }
        """;

    [TestInitialize]
    public void Setup()
    {
        ResetCache();
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
        ResetCache();
        _context?.Dispose();
        _connection?.Dispose();
    }

    private static void ResetCache()
    {
        var field = typeof(MovieAccess).GetField("_trendingCache", BindingFlags.NonPublic | BindingFlags.Static);
        field?.SetValue(null, null);
    }

    private MovieAccess CreateAccess(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var client = new HttpClient(new FakeHttpMessageHandler(json, status))
        {
            BaseAddress = new Uri("https://fake.imdb.local/")
        };
        return new MovieAccess(_context!, client);
    }

    // --- GetTrendingMovies ---

    [TestMethod]
    public void GetTrendingMovies_ReturnsOnlyMovies()
    {
        var access = CreateAccess(ValidTrendingJson);
        var result = access.GetTrendingMovies();
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(m => !string.IsNullOrWhiteSpace(m.Id)));
    }

    [TestMethod]
    public void GetTrendingMovies_MapsRuntimeAndRating()
    {
        var access = CreateAccess(ValidTrendingJson);
        var result = access.GetTrendingMovies();
        var film = result.First(m => m.Id == "m1");
        Assert.AreEqual(5400, film.RuntimeSeconds);
        Assert.AreEqual(7.5, film.Rating);
    }

    [TestMethod]
    public void GetTrendingMovies_CachesResult()
    {
        var access = CreateAccess(ValidTrendingJson);
        var first = access.GetTrendingMovies();
        var second = access.GetTrendingMovies();
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void GetTrendingMovies_EmptyResponse_ReturnsEmptyList()
    {
        var access = CreateAccess("""{"titles": []}""");
        var result = access.GetTrendingMovies();
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTrendingMovies_HttpError_ThrowsHttpRequestException()
    {
        var access = CreateAccess("{}", HttpStatusCode.InternalServerError);
        Assert.ThrowsException<HttpRequestException>(() => access.GetTrendingMovies());
    }

    // --- GetMovieById ---

    [TestMethod]
    public void GetMovieById_EmptyId_ThrowsArgumentException()
    {
        var access = CreateAccess("{}");
        Assert.ThrowsException<ArgumentException>(() => access.GetMovieById(""));
    }

    [TestMethod]
    public void GetMovieById_WhitespaceId_ThrowsArgumentException()
    {
        var access = CreateAccess("{}");
        Assert.ThrowsException<ArgumentException>(() => access.GetMovieById("   "));
    }

    [TestMethod]
    public void GetMovieById_ExistingInDb_ReturnsCachedRow()
    {
        _context!.Movies.Add(new Movie { Id = "m1", Title = "Cached", Description = "x", RuntimeSeconds = 5400 });
        _context.SaveChanges();

        var access = CreateAccess("{}");
        var result = access.GetMovieById("m1");
        Assert.IsNotNull(result);
        Assert.AreEqual("Cached", result!.Title);
    }

    [TestMethod]
    public void GetMovieById_NotFoundApi_ReturnsNull()
    {
        var access = CreateAccess("{}", HttpStatusCode.NotFound);
        var result = access.GetMovieById("unknown");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetMovieById_NonMovieType_ReturnsNull()
    {
        var json = """{"id":"tv1","type":"tvSeries","primaryTitle":"Show","plot":"p","genres":[],"runtimeSeconds":1800}""";
        var access = CreateAccess(json);
        var result = access.GetMovieById("tv1");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetMovieById_ValidMovie_PersistsAndReturns()
    {
        var json = """{"id":"m99","type":"movie","primaryTitle":"New Movie","plot":"p","genres":["Drama"],"runtimeSeconds":7200}""";
        var access = CreateAccess(json);
        var result = access.GetMovieById("m99");
        Assert.IsNotNull(result);
        Assert.AreEqual("New Movie", result!.Title);
        Assert.IsNotNull(_context!.Movies.Find("m99"));
    }

    // --- EnsureMoviesPersisted ---

    [TestMethod]
    public void EnsureMoviesPersisted_AddsNewMovies()
    {
        var access = CreateAccess("{}");
        var movies = new List<Movie>
        {
            new() { Id = "p1", Title = "Film A", Description = "d", RuntimeSeconds = 3600 },
            new() { Id = "p2", Title = "Film B", Description = "d", RuntimeSeconds = 4800 }
        };
        access.EnsureMoviesPersisted(movies);
        Assert.AreEqual(2, _context!.Movies.Count());
    }

    [TestMethod]
    public void EnsureMoviesPersisted_SkipsAlreadyPersisted()
    {
        _context!.Movies.Add(new Movie { Id = "p1", Title = "Film A", Description = "d", RuntimeSeconds = 3600 });
        _context.SaveChanges();

        var access = CreateAccess("{}");
        access.EnsureMoviesPersisted(new[]
        {
            new Movie { Id = "p1", Title = "Film A Updated", Description = "d", RuntimeSeconds = 3600 },
            new Movie { Id = "p2", Title = "Film B", Description = "d", RuntimeSeconds = 4800 }
        });
        Assert.AreEqual(2, _context.Movies.Count());
        Assert.AreEqual("Film A", _context.Movies.Find("p1")!.Title);
    }

    [TestMethod]
    public void EnsureMoviesPersisted_CollapsesDuplicateIds()
    {
        var access = CreateAccess("{}");
        var movies = new[]
        {
            new Movie { Id = "dup", Title = "First", Description = "d", RuntimeSeconds = 3600 },
            new Movie { Id = "dup", Title = "Second", Description = "d", RuntimeSeconds = 3600 }
        };
        access.EnsureMoviesPersisted(movies);
        Assert.AreEqual(1, _context!.Movies.Count(m => m.Id == "dup"));
    }

    [TestMethod]
    public void EnsureMoviesPersisted_EmptyList_DoesNothing()
    {
        var access = CreateAccess("{}");
        access.EnsureMoviesPersisted(Array.Empty<Movie>());
        Assert.AreEqual(0, _context!.Movies.Count());
    }

    // --- Search / SearchSchedule ---

    [TestMethod]
    public void Search_ReturnsMatchingMovies()
    {
        _context!.Movies.AddRange(
            new Movie { Id = "s1", Title = "The Matrix", Description = "d", RuntimeSeconds = 5400 },
            new Movie { Id = "s2", Title = "Inception",  Description = "d", RuntimeSeconds = 6000 }
        );
        _context.SaveChanges();

        var access = CreateAccess("{}");
        var result = access.Search("Matrix");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("The Matrix", result[0].Title);
    }

    [TestMethod]
    public void SearchSchedule_ReturnsFilteredByTitle()
    {
        var schedules = new List<MovieSchedule>
        {
            new() { Movie = new Movie { Id = "s1", Title = "The Matrix", Description = "d" }, AuditoriumId = 1,
                    ScheduleDate = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(11,0) },
            new() { Movie = new Movie { Id = "s2", Title = "Inception",  Description = "d" }, AuditoriumId = 1,
                    ScheduleDate = DateOnly.FromDateTime(DateTime.Today), StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(14,0) }
        };

        var access = CreateAccess("{}");
        var result = access.SearchSchedule(schedules, "Matrix");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("The Matrix", result[0].Movie.Title);
    }
}
