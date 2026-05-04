using Microsoft.EntityFrameworkCore;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class BookingSnackAccess
{
    private readonly AppDbContext _context;

    public BookingSnackAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<BookingSnack> GetAll()
    {
        return _context.BookingSnacks.Include(bs => bs.Snack).ToList();
    }

    public BookingSnack? GetById(int id)
    {
        return _context.BookingSnacks.Include(bs => bs.Snack).FirstOrDefault(bs => bs.Id == id);
    }

    public void Add(BookingSnack bookingSnack)
    {
        _context.BookingSnacks.Add(bookingSnack);
        _context.SaveChanges();
    }

    public void Update(BookingSnack bookingSnack)
    {
        _context.BookingSnacks.Update(bookingSnack);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var bookingSnack = _context.BookingSnacks.Find(id);
        if (bookingSnack != null)
        {
            _context.BookingSnacks.Remove(bookingSnack);
            _context.SaveChanges();
        }
    }
}
