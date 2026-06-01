using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class MovieLogic
{
    public const string OpeningTime = LocationOpeningTimeLogic.DefaultOpeningTime;
    public const string ClosingTime = LocationOpeningTimeLogic.DefaultClosingTime;
    public const int MinimumBreakMinutes = 30;

    private readonly MovieAccess _movieAccess;
    private readonly MovieScheduleAccess _movieScheduleAccess;
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly LocationOpeningTimeLogic _locationOpeningTimeLogic;

    public MovieLogic(
        MovieAccess movieAccess,
        MovieScheduleAccess movieScheduleAccess,
        AuditoriumLogic auditoriumLogic,
        LocationOpeningTimeLogic locationOpeningTimeLogic)
    {
        _movieAccess = movieAccess;
        _movieScheduleAccess = movieScheduleAccess;
        _auditoriumLogic = auditoriumLogic;
        _locationOpeningTimeLogic = locationOpeningTimeLogic;
    }

    public IReadOnlyList<MovieSchedule> GetScheduleForDate(DateOnly date)
    {
        return _movieScheduleAccess.GetScheduleForDate(date);
    }

    public List<Movie> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        return _movieAccess.Search(query);
    }

    public List<MovieSchedule> SearchSchedule(IReadOnlyList<MovieSchedule> schedule, string query)
    {
        return _movieAccess.SearchSchedule(schedule, query);
    }

    public IReadOnlyList<MovieSchedule> GetOrGenerateSchedule(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date < today)
        {
            return GetScheduleForDate(date);
        }

        return GenerateSchedule(date);
    }

    public IReadOnlyList<MovieSchedule> GenerateSchedule(DateOnly date)
    {
        var existingSchedule = GetScheduleForDate(date);
        var scheduledAuditoriumIds = existingSchedule
            .Select(schedule => schedule.AuditoriumId)
            .ToHashSet();

        var auditoriums = _auditoriumLogic.GetAll()
            .Where(auditorium => !scheduledAuditoriumIds.Contains(auditorium.Id))
            .ToList();

        if (auditoriums.Count == 0)
        {
            return existingSchedule;
        }

        var openAuditoriums = auditoriums
            .Select(auditorium => new
            {
                Auditorium = auditorium,
                OpeningHours = _locationOpeningTimeLogic.GetOpeningHoursForDate(auditorium.LocationId, date)
            })
            .Where(entry => !entry.OpeningHours.IsClosed
                            && entry.OpeningHours.OpeningTime.HasValue
                            && entry.OpeningHours.ClosingTime.HasValue)
            .ToList();

        if (openAuditoriums.Count == 0)
        {
            return existingSchedule;
        }

        var validMovies = _movieAccess.GetTrendingMovies()
            .Where(movie => !string.IsNullOrWhiteSpace(movie.Id) && movie.RuntimeSeconds is > 0)
            .GroupBy(movie => movie.Id)
            .Select(group => group.First())
            .ToList();

        if (validMovies.Count == 0)
        {
            throw new InvalidOperationException("No trending movies are available with a valid runtime.");
        }

        Shuffle(validMovies);

        var schedules = new List<MovieSchedule>();
        foreach (var entry in openAuditoriums)
        {
            var auditorium = entry.Auditorium;
            var openingTime = entry.OpeningHours.OpeningTime!.Value;
            var closingTime = entry.OpeningHours.ClosingTime!.Value;
            var usedMovieIds = new HashSet<string>(StringComparer.Ordinal);
            var currentStart = openingTime;

            while (true)
            {
                var unusedCandidates = validMovies
                    .Where(movie => !usedMovieIds.Contains(movie.Id) && FitsInRemainingWindow(movie, currentStart, closingTime))
                    .ToList();

                var selectedMovie = PickRandom(unusedCandidates);

                if (selectedMovie == null)
                {
                    var repeatedCandidates = validMovies
                        .Where(movie => usedMovieIds.Contains(movie.Id) && FitsInRemainingWindow(movie, currentStart, closingTime))
                        .ToList();

                    selectedMovie = PickRandom(repeatedCandidates);
                }

                if (selectedMovie == null)
                {
                    break;
                }

                var endTime = currentStart.Add(TimeSpan.FromSeconds(selectedMovie.RuntimeSeconds!.Value));
                schedules.Add(new MovieSchedule
                {
                    ScheduleDate = date,
                    AuditoriumId = auditorium.Id,
                    Auditorium = auditorium,
                    MovieId = selectedMovie.Id,
                    StartTime = currentStart,
                    EndTime = endTime,
                    Movie = selectedMovie
                });

                usedMovieIds.Add(selectedMovie.Id);
                currentStart = endTime.AddMinutes(MinimumBreakMinutes);
            }
        }

        if (schedules.Count == 0)
        {
            return existingSchedule;
        }

        var scheduledMovies = schedules.Select(schedule => schedule.Movie).ToList();
        _movieAccess.EnsureMoviesPersisted(scheduledMovies);
        _movieScheduleAccess.AddSchedules(schedules);

        return GetScheduleForDate(date);
    }

    private static bool FitsInRemainingWindow(Movie movie, TimeOnly currentStart, TimeOnly closingTime)
    {
        return movie.RuntimeSeconds is > 0
               && currentStart.Add(TimeSpan.FromSeconds(movie.RuntimeSeconds.Value)) <= closingTime;
    }

    private static Movie? PickRandom(IReadOnlyList<Movie> candidates)
    {
        return candidates.Count == 0
            ? null
            : candidates[Random.Shared.Next(candidates.Count)];
    }

    private static void Shuffle(IList<Movie> movies)
    {
        for (var index = movies.Count - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (movies[index], movies[swapIndex]) = (movies[swapIndex], movies[index]);
        }
    }
}
