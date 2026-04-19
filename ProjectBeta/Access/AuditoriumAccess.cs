using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.EntityFrameworkCore;

namespace ProjectBeta.Access;

public class AuditoriumAccess
{
    private readonly AppDbContext _context;

    public AuditoriumAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<Auditorium> GetAll()
    {
        return _context.Auditoriums.ToList();
    }

    public Auditorium? GetById(int id)
    {
        return _context.Auditoriums
            .Include(a => a.Cinema)
            .FirstOrDefault(a => a.Id == id);
    }

    public void UpdateName(int id, string newName)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.Name = newName;
        _context.SaveChanges();
    }

    public List<Auditorium> GetByCinemaId(int cinemaId)
    {
        return _context.Auditoriums
            .Where(a => a.CinemaId == cinemaId)
            .ToList();
    }

    public void UpdateCapacity(int id, int newCapacity)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.Capacity = newCapacity;
        _context.SaveChanges();
    }

    public void UpdateCinema(int id, int newCinemaId)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.CinemaId = newCinemaId;
        _context.SaveChanges();
    }
}