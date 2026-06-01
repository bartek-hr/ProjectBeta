using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class LocationOpeningTimeAccess
{
    private readonly AppDbContext _context;

    public LocationOpeningTimeAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<LocationOpeningTime> GetByLocationId(int locationId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return _context.LocationOpeningTimes
            .Where(openingTime => openingTime.LocationId == locationId)
            .Where(openingTime => openingTime.ExpiresAt >= today)
            .OrderByDescending(openingTime => openingTime.StartDate)
            .ThenByDescending(openingTime => openingTime.CreatedAt)
            .ThenByDescending(openingTime => openingTime.Id)
            .ToList();
    }

    public LocationOpeningTime? GetActiveForDate(int locationId, DateOnly date)
    {
        return _context.LocationOpeningTimes
            .Where(openingTime =>
                openingTime.LocationId == locationId
                && openingTime.StartDate <= date
                && openingTime.ExpiresAt >= date)
            .OrderByDescending(openingTime => openingTime.CreatedAt)
            .ThenByDescending(openingTime => openingTime.Id)
            .FirstOrDefault();
    }

    public bool HasDefaultForLocation(int locationId)
    {
        return _context.LocationOpeningTimes.Any(openingTime =>
            openingTime.LocationId == locationId
            && openingTime.StartDate == DateOnly.MinValue
            && openingTime.ExpiresAt == DateOnly.MaxValue);
    }

    public void Add(LocationOpeningTime openingTime)
    {
        _context.LocationOpeningTimes.Add(openingTime);
        _context.SaveChanges();
    }
}
