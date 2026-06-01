using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class LocationOpeningTimeLogic
{
    public const string DefaultOpeningTime = "09:00";
    public const string DefaultClosingTime = "20:00";

    private static readonly TimeOnly DefaultOpeningTimeValue = TimeOnly.ParseExact(DefaultOpeningTime, "HH:mm");
    private static readonly TimeOnly DefaultClosingTimeValue = TimeOnly.ParseExact(DefaultClosingTime, "HH:mm");

    private readonly LocationOpeningTimeAccess _openingTimeAccess;
    private readonly LocationAccess _locationAccess;
    private readonly MovieScheduleAccess _movieScheduleAccess;

    public LocationOpeningTimeLogic(
        LocationOpeningTimeAccess openingTimeAccess,
        LocationAccess locationAccess,
        MovieScheduleAccess movieScheduleAccess)
    {
        _openingTimeAccess = openingTimeAccess;
        _locationAccess = locationAccess;
        _movieScheduleAccess = movieScheduleAccess;
    }

    public List<LocationOpeningTime> GetByLocationId(int locationId)
    {
        EnsureDefaultOpeningTime(locationId);
        return _openingTimeAccess.GetByLocationId(locationId);
    }

    public LocationOpeningHours GetOpeningHoursForDate(int locationId, DateOnly date)
    {
        EnsureDefaultOpeningTime(locationId);

        var openingTime = _openingTimeAccess.GetActiveForDate(locationId, date);
        if (openingTime == null)
        {
            return GetDefaultOpeningHours();
        }

        return new LocationOpeningHours(openingTime.OpeningTime, openingTime.ClosingTime);
    }

    public void Add(LocationOpeningTime openingTime, User currentUser)
    {
        if (!currentUser.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only admins can add location opening times.");
        }

        Validate(openingTime);

        var location = _locationAccess.GetById(openingTime.LocationId);
        if (location == null)
        {
            throw new InvalidOperationException("Location not found.");
        }

        openingTime.CreatedAt = DateTime.UtcNow;
        _openingTimeAccess.Add(openingTime);
        _movieScheduleAccess.DeleteForLocationDateRange(openingTime.LocationId, openingTime.StartDate, openingTime.ExpiresAt);
    }

    public static LocationOpeningHours GetDefaultOpeningHours()
    {
        return new LocationOpeningHours(DefaultOpeningTimeValue, DefaultClosingTimeValue);
    }

    private static void Validate(LocationOpeningTime openingTime)
    {
        if (openingTime.StartDate > openingTime.ExpiresAt)
        {
            throw new InvalidOperationException("Start date must be on or before expires at.");
        }

        var hasOpeningTime = openingTime.OpeningTime.HasValue;
        var hasClosingTime = openingTime.ClosingTime.HasValue;
        if (hasOpeningTime != hasClosingTime)
        {
            throw new InvalidOperationException("Opening time and closing time must both be set, or both be empty for a closed location.");
        }

        if (hasOpeningTime && openingTime.OpeningTime >= openingTime.ClosingTime)
        {
            throw new InvalidOperationException("Opening time must be before closing time.");
        }
    }

    private void EnsureDefaultOpeningTime(int locationId)
    {
        if (_openingTimeAccess.HasDefaultForLocation(locationId))
        {
            return;
        }

        var location = _locationAccess.GetById(locationId);
        if (location == null)
        {
            return;
        }

        _openingTimeAccess.Add(new LocationOpeningTime
        {
            LocationId = locationId,
            StartDate = DateOnly.MinValue,
            ExpiresAt = DateOnly.MaxValue,
            OpeningTime = DefaultOpeningTimeValue,
            ClosingTime = DefaultClosingTimeValue,
            CreatedAt = DateTime.MinValue
        });
    }
}
