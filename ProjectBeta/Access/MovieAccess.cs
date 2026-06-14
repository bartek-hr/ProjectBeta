using System.Runtime.ExceptionServices;
using System.Text.Json;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class MovieAccess
{
    private const string TrendingMoviesEndpoint = "titles?types=MOVIE&countryCodes=NL&sortBy=SORT_BY_POPULARITY&sortOrder=ASC";
    private const string TrendingMoviesBackupFileName = "trending-movies-backup.json";
    private const string ImdbApiHost = "api.imdbapi.dev";

    private static readonly object TrendingLock = new();
    private static IReadOnlyList<Movie>? _trendingCache;

    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MovieAccess(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    public IReadOnlyList<Movie> GetTrendingMovies()
    {
        if (_trendingCache != null)
        {
            return _trendingCache;
        }

        lock (TrendingLock)
        {
            if (_trendingCache != null)
            {
                return _trendingCache;
            }

            var content = GetTrendingMoviesResponseContent();

            var payload = JsonSerializer.Deserialize<TrendingTitlesResponse>(content, _jsonOptions)
                ?? throw new JsonException("IMDb API returned an empty trending response.");

            _trendingCache = payload.Titles?
                .Where(title => string.Equals(title.Type, "movie", StringComparison.OrdinalIgnoreCase))
                .Select(MapToMovie)
                .ToList()
                ?? [];

            return _trendingCache;
        }
    }

    private string GetTrendingMoviesResponseContent()
    {
        try
        {
            var response = _httpClient
                .GetAsync(TrendingMoviesEndpoint)
                .GetAwaiter()
                .GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var exception = new HttpRequestException(
                    $"IMDb API returned {(int)response.StatusCode} ({response.ReasonPhrase}).",
                    null,
                    response.StatusCode);
                return ReadTrendingMoviesBackupOrThrow(exception);
            }

            return response.Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch (HttpRequestException exception)
        {
            return ReadTrendingMoviesBackupOrThrow(exception);
        }
        catch (TaskCanceledException exception)
        {
            return ReadTrendingMoviesBackupOrThrow(exception);
        }
    }

    private string ReadTrendingMoviesBackupOrThrow(Exception originalException)
    {
        if (!CanUseTrendingMoviesBackup())
        {
            ExceptionDispatchInfo.Capture(originalException).Throw();
        }

        var backupPath = GetTrendingMoviesBackupPath();
        if (backupPath == null)
        {
            ExceptionDispatchInfo.Capture(originalException).Throw();
        }

        return File.ReadAllText(backupPath);
    }

    private bool CanUseTrendingMoviesBackup()
    {
        return string.Equals(_httpClient.BaseAddress?.Host, ImdbApiHost, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetTrendingMoviesBackupPath()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", TrendingMoviesBackupFileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", TrendingMoviesBackupFileName)
        };

        return candidatePaths.FirstOrDefault(File.Exists);
    }

    public Movie? GetMovieById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Movie id cannot be empty.", nameof(id));
        }

        var trimmedId = id.Trim();
        var movie = _context.Movies.Find(trimmedId);

        if (movie != null)
        {
            return movie;
        }

        var response = _httpClient.GetAsync($"titles/{trimmedId}").GetAwaiter().GetResult();

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var content = response.Content
            .ReadAsStringAsync()
            .GetAwaiter()
            .GetResult();

        var payload = JsonSerializer.Deserialize<TitleResponse>(content, _jsonOptions)
            ?? throw new JsonException("IMDb API returned an empty title response.");

        if (!string.Equals(payload.Type, "movie", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        movie = MapToMovie(payload);
        _context.Movies.Add(movie);
        _context.SaveChanges();

        return movie;
    }

    public List<Movie> Search(string query)
    {
        return _context.Movies
            .Where(m => m.Title.Contains(query))
            .ToList();
    }

    public List<MovieSchedule> SearchSchedule(IReadOnlyList<MovieSchedule> schedule, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return schedule.ToList();

        return schedule
            .Where(s => s.Movie.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Movie> GetAll()
    {
        return _context.Movies.OrderBy(m => m.Title).ToList();
    }

    public Movie Add(Movie movie)
    {
        if (string.IsNullOrWhiteSpace(movie.Id))
            movie.Id = Guid.NewGuid().ToString();

        _context.Movies.Add(movie);
        _context.SaveChanges();
        return movie;
    }

    public Movie Update(Movie movie)
    {
        _context.Movies.Update(movie);
        _context.SaveChanges();
        return movie;
    }

    public void Delete(string id)
    {
        var movie = _context.Movies.Find(id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            _context.SaveChanges();
        }
    }

    public void EnsureMoviesPersisted(IEnumerable<Movie> movies)
    {
        var candidates = movies
            .Where(movie => !string.IsNullOrWhiteSpace(movie.Id))
            .GroupBy(movie => movie.Id)
            .Select(group => group.First())
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var candidateIds = candidates.Select(movie => movie.Id).ToList();
        var existingIds = _context.Movies
            .Where(movie => candidateIds.Contains(movie.Id))
            .Select(movie => movie.Id)
            .ToHashSet();

        var missingMovies = candidates
            .Where(movie => !existingIds.Contains(movie.Id))
            .Select(movie => new Movie
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Genres = movie.Genres.ToList(),
                Rating = movie.Rating,
                RuntimeSeconds = movie.RuntimeSeconds
            })
            .ToList();

        if (missingMovies.Count == 0)
        {
            return;
        }

        _context.Movies.AddRange(missingMovies);
        _context.SaveChanges();
    }

    private static Movie MapToMovie(TitleResponse title)
    {
        return new Movie
        {
            Id = title.Id ?? string.Empty,
            Title = title.PrimaryTitle ?? string.Empty,
            Description = title.Plot ?? string.Empty,
            Genres = title.Genres ?? [],
            Rating = title.Rating?.AggregateRating,
            RuntimeSeconds = title.RuntimeSeconds
        };
    }

    private sealed class TrendingTitlesResponse
    {
        public List<TitleResponse>? Titles { get; set; }
    }

    private sealed class TitleResponse
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? PrimaryTitle { get; set; }
        public string? Plot { get; set; }
        public List<string>? Genres { get; set; }
        public RatingResponse? Rating { get; set; }
        public int? RuntimeSeconds { get; set; }
    }

    private sealed class RatingResponse
    {
        public double? AggregateRating { get; set; }
    }
}
