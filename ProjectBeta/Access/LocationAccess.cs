using ProjectBeta.Data;
using ProjectBeta.Model;
using Microsoft.EntityFrameworkCore;

namespace ProjectBeta.Access;

public class LocationAccess
{
    private readonly AppDbContext _context;

    public LocationAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<Location> GetAll()
    {
        return _context.Locations.Include(l => l.Auditoriums).ToList();
    }

    public Location? GetById(int id)
    {
        return _context.Locations
            .Include(l => l.Auditoriums)
            .FirstOrDefault(l => l.Id == id);
    }

    public void Add(Location location)
    {
        _context.Locations.Add(location);
        _context.SaveChanges();
    }

    public void Update(Location location)
    {
        _context.Locations.Update(location);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var location = _context.Locations.Find(id);
        if (location == null) return;
        _context.Locations.Remove(location);
        _context.SaveChanges();
    }

    public void UpdateCapacity(int id, int newCapacity)
    {
        var location = _context.Locations.Find(id);
        if (location == null) return;
        location.Capacity = newCapacity;
        _context.SaveChanges();
    }

    public List<Location> Search(string query)
    {
        return _context.Locations
            .Include(l => l.Auditoriums)
            .Where(l => l.Name.Contains(query))
            .ToList();
    }
}
