using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class MovieScheduleAccess
{
    private readonly AppDbContext _context;

    public MovieScheduleAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<MovieSchedule> GetScheduleForDate(DateOnly date)
    {
        return _context.MovieSchedules
            .Include(schedule => schedule.Movie)
            .Include(schedule => schedule.Auditorium)
            .Where(schedule => schedule.ScheduleDate == date)
            .OrderBy(schedule => schedule.AuditoriumId)
            .ThenBy(schedule => schedule.StartTime)
            .ToList();
    }

    public void AddSchedules(IEnumerable<MovieSchedule> schedules)
    {
        var scheduleEntities = schedules
            .Select(schedule => new MovieSchedule
            {
                ScheduleDate = schedule.ScheduleDate,
                AuditoriumId = schedule.AuditoriumId,
                MovieId = schedule.MovieId,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime
            })
            .ToList();

        _context.MovieSchedules.AddRange(scheduleEntities);
        _context.SaveChanges();
    }

    public bool HasScheduleForDate(DateOnly date)
    {
        return _context.MovieSchedules.Any(schedule => schedule.ScheduleDate == date);
    }
}
