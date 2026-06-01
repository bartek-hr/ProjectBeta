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

    public void Add(Auditorium auditorium)
    {
        _context.Auditoriums.Add(auditorium);
        _context.SaveChanges();
    }

    public List<Auditorium> GetAll()
    {
        return _context.Auditoriums.ToList();
    }

    public Auditorium? GetById(int id)
    {
        return _context.Auditoriums
            .Include(a => a.Location)
            .FirstOrDefault(a => a.Id == id);
    }

    public void UpdateName(int id, string newName)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.Name = newName;
        _context.SaveChanges();
    }

    public List<Auditorium> GetByLocationId(int locationId)
    {
        return _context.Auditoriums
            .Where(a => a.LocationId == locationId)
            .ToList();
    }

    public void UpdateCapacity(int id, int newCapacity)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.Capacity = newCapacity;
        _context.SaveChanges();
    }

    public void UpdateLocation(int id, int newLocationId)
    {
        var auditorium = _context.Auditoriums.Find(id);
        if (auditorium == null) return;
        auditorium.LocationId = newLocationId;
        _context.SaveChanges();
    }
}
