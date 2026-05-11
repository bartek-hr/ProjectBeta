using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class MovieLogic
{
    public const string OpeningTime = "09:00";
    public const string ClosingTime = "20:00";
    public const int MinimumBreakMinutes = 30;

    private static readonly TimeOnly OpeningTimeValue = TimeOnly.ParseExact(OpeningTime, "HH:mm");
    private static readonly TimeOnly ClosingTimeValue = TimeOnly.ParseExact(ClosingTime, "HH:mm");

    private readonly MovieAccess _movieAccess;
    private readonly MovieScheduleAccess _movieScheduleAccess;
    private readonly AuditoriumLogic _auditoriumLogic;

    public MovieLogic(MovieAccess movieAccess, MovieScheduleAccess movieScheduleAccess, AuditoriumLogic auditoriumLogic)
    {
        _movieAccess = movieAccess;
        _movieScheduleAccess = movieScheduleAccess;
        _auditoriumLogic = auditoriumLogic;
    }

    public IReadOnlyList<MovieSchedule> GetScheduleForDate(DateOnly date)
    {
        return _movieScheduleAccess.GetScheduleForDate(date);
    }

    public IReadOnlyList<MovieSchedule> GetOrGenerateSchedule(DateOnly date)
    {
        var existingSchedule = GetScheduleForDate(date);
        if (existingSchedule.Count > 0)
        {
            return existingSchedule;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date < today)
        {
            return [];
        }

        return GenerateSchedule(date);
    }

    public IReadOnlyList<MovieSchedule> GenerateSchedule(DateOnly date)
    {
        var existingSchedule = GetScheduleForDate(date);
        if (existingSchedule.Count > 0)
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

        var auditoriums = _auditoriumLogic.GetAll();
        if (auditoriums.Count == 0)
        {
            return [];
        }

        Shuffle(validMovies);

        var schedules = new List<MovieSchedule>();
        foreach (var auditorium in auditoriums)
        {
            var usedMovieIds = new HashSet<string>(StringComparer.Ordinal);
            var currentStart = OpeningTimeValue;

            while (true)
            {
                var unusedCandidates = validMovies
                    .Where(movie => !usedMovieIds.Contains(movie.Id) && FitsInRemainingWindow(movie, currentStart))
                    .ToList();

                var selectedMovie = PickRandom(unusedCandidates);

                if (selectedMovie == null)
                {
                    var repeatedCandidates = validMovies
                        .Where(movie => usedMovieIds.Contains(movie.Id) && FitsInRemainingWindow(movie, currentStart))
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
            return [];
        }

        var scheduledMovies = schedules.Select(schedule => schedule.Movie).ToList();
        _movieAccess.EnsureMoviesPersisted(scheduledMovies);
        _movieScheduleAccess.AddSchedules(schedules);

        return GetScheduleForDate(date);
    }

    private static bool FitsInRemainingWindow(Movie movie, TimeOnly currentStart)
    {
        return movie.RuntimeSeconds is > 0
               && currentStart.Add(TimeSpan.FromSeconds(movie.RuntimeSeconds.Value)) <= ClosingTimeValue;
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
