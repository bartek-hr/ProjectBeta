using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class CinemaAccess
{
    private readonly AppDbContext _context;

    public CinemaAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<Cinema> GetAll()
    {
        return _context.Cinemas.ToList();
    }

    public Cinema? GetById(int id)
    {
        return _context.Cinemas
            .Include(c => c.Auditoriums)
            .FirstOrDefault(c => c.Id == id);
    }

    public void UpdateName(int id, string newName)
    {
        var cinema = _context.Cinemas.Find(id);
        if (cinema == null) return;
        cinema.Name = newName;
        _context.SaveChanges();
    }

    public void UpdateCity(int id, string newCity)
    {
        var cinema = _context.Cinemas.Find(id);
        if (cinema == null) return;
        cinema.City = newCity;
        _context.SaveChanges();
    }
}
