using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class CinemaOpeningTimeLogic
{
    public const string DefaultOpeningTime = "09:00";
    public const string DefaultClosingTime = "20:00";

    private static readonly TimeOnly DefaultOpeningTimeValue = TimeOnly.ParseExact(DefaultOpeningTime, "HH:mm");
    private static readonly TimeOnly DefaultClosingTimeValue = TimeOnly.ParseExact(DefaultClosingTime, "HH:mm");

    private readonly CinemaOpeningTimeAccess _openingTimeAccess;
    private readonly CinemaAccess _cinemaAccess;
    private readonly MovieScheduleAccess _movieScheduleAccess;

    public CinemaOpeningTimeLogic(
        CinemaOpeningTimeAccess openingTimeAccess,
        CinemaAccess cinemaAccess,
        MovieScheduleAccess movieScheduleAccess)
    {
        _openingTimeAccess = openingTimeAccess;
        _cinemaAccess = cinemaAccess;
        _movieScheduleAccess = movieScheduleAccess;
    }

    public List<CinemaOpeningTime> GetByCinemaId(int cinemaId)
    {
        EnsureDefaultOpeningTime(cinemaId);
        return _openingTimeAccess.GetByCinemaId(cinemaId);
    }

    public CinemaOpeningHours GetOpeningHoursForDate(int cinemaId, DateOnly date)
    {
        EnsureDefaultOpeningTime(cinemaId);

        var openingTime = _openingTimeAccess.GetActiveForDate(cinemaId, date);
        if (openingTime == null)
        {
            return GetDefaultOpeningHours();
        }

        return new CinemaOpeningHours(openingTime.OpeningTime, openingTime.ClosingTime);
    }

    public void Add(CinemaOpeningTime openingTime, User currentUser)
    {
        if (!currentUser.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only admins can add cinema opening times.");
        }

        Validate(openingTime);

        var cinema = _cinemaAccess.GetById(openingTime.CinemaId);
        if (cinema == null)
        {
            throw new InvalidOperationException("Cinema not found.");
        }

        openingTime.CreatedAt = DateTime.UtcNow;
        _openingTimeAccess.Add(openingTime);
        _movieScheduleAccess.DeleteForCinemaDateRange(openingTime.CinemaId, openingTime.StartDate, openingTime.ExpiresAt);
    }

    public static CinemaOpeningHours GetDefaultOpeningHours()
    {
        return new CinemaOpeningHours(DefaultOpeningTimeValue, DefaultClosingTimeValue);
    }

    private static void Validate(CinemaOpeningTime openingTime)
    {
        if (openingTime.StartDate > openingTime.ExpiresAt)
        {
            throw new InvalidOperationException("Start date must be on or before expires at.");
        }

        var hasOpeningTime = openingTime.OpeningTime.HasValue;
        var hasClosingTime = openingTime.ClosingTime.HasValue;
        if (hasOpeningTime != hasClosingTime)
        {
            throw new InvalidOperationException("Opening time and closing time must both be set, or both be empty for a closed cinema.");
        }

        if (hasOpeningTime && openingTime.OpeningTime >= openingTime.ClosingTime)
        {
            throw new InvalidOperationException("Opening time must be before closing time.");
        }
    }

    private void EnsureDefaultOpeningTime(int cinemaId)
    {
        if (_openingTimeAccess.HasDefaultForCinema(cinemaId))
        {
            return;
        }

        var cinema = _cinemaAccess.GetById(cinemaId);
        if (cinema == null)
        {
            return;
        }

        _openingTimeAccess.Add(new CinemaOpeningTime
        {
            CinemaId = cinemaId,
            StartDate = DateOnly.MinValue,
            ExpiresAt = DateOnly.MaxValue,
            OpeningTime = DefaultOpeningTimeValue,
            ClosingTime = DefaultClosingTimeValue,
            CreatedAt = DateTime.MinValue
        });
    }
}
