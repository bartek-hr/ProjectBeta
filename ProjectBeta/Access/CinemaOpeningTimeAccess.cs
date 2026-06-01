using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class CinemaOpeningTimeAccess
{
    private readonly AppDbContext _context;

    public CinemaOpeningTimeAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<CinemaOpeningTime> GetByCinemaId(int cinemaId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return _context.CinemaOpeningTimes
            .Where(openingTime => openingTime.CinemaId == cinemaId)
            .Where(openingTime => openingTime.ExpiresAt >= today)
            .OrderByDescending(openingTime => openingTime.StartDate)
            .ThenByDescending(openingTime => openingTime.CreatedAt)
            .ThenByDescending(openingTime => openingTime.Id)
            .ToList();
    }

    public CinemaOpeningTime? GetActiveForDate(int cinemaId, DateOnly date)
    {
        return _context.CinemaOpeningTimes
            .Where(openingTime =>
                openingTime.CinemaId == cinemaId
                && openingTime.StartDate <= date
                && openingTime.ExpiresAt >= date)
            .OrderByDescending(openingTime => openingTime.CreatedAt)
            .ThenByDescending(openingTime => openingTime.Id)
            .FirstOrDefault();
    }

    public bool HasDefaultForCinema(int cinemaId)
    {
        return _context.CinemaOpeningTimes.Any(openingTime =>
            openingTime.CinemaId == cinemaId
            && openingTime.StartDate == DateOnly.MinValue
            && openingTime.ExpiresAt == DateOnly.MaxValue);
    }

    public void Add(CinemaOpeningTime openingTime)
    {
        _context.CinemaOpeningTimes.Add(openingTime);
        _context.SaveChanges();
    }
}
