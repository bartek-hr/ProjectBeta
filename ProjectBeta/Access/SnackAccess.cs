using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class SnackAccess
{
    private readonly AppDbContext _context;

    public SnackAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<Snack> GetAll()
    {
        return _context.Snacks.ToList();
    }

    public Snack? GetById(int id)
    {
        return _context.Snacks.FirstOrDefault(s => s.Id == id);
    }

    public void Add(Snack snack)
    {
        _context.Snacks.Add(snack);
        _context.SaveChanges();
    }

    public void Update(Snack snack)
    {
        _context.Snacks.Update(snack);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var snack = _context.Snacks.Find(id);
        if (snack != null)
        {
            _context.Snacks.Remove(snack);
            _context.SaveChanges();
        }
    }
}
