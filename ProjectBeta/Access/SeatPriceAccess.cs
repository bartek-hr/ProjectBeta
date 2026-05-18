using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class SeatPriceAccess
{
    private readonly AppDbContext _context;

    public SeatPriceAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<SeatPrice> GetAll() => _context.SeatPrices.ToList();

    public SeatPrice? GetById(int id) => _context.SeatPrices.Find(id);

    public void UpdatePrice(int id, decimal newPrice)
    {
        var seatPrice = _context.SeatPrices.Find(id);
        if (seatPrice == null) return;
        seatPrice.Price = newPrice;
        _context.SaveChanges();
    }
}
