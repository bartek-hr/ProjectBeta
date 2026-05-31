using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class DiscountAccess
{
    private readonly AppDbContext _context;

    public DiscountAccess(AppDbContext context)
    {
        _context = context;
    }

    public List<Discount> GetActive() =>
        _context.Discounts.Where(d => d.IsActive).ToList();

    public void Add(Discount discount)
    {
        _context.Discounts.Add(discount);
        _context.SaveChanges();
    }

    public void Update(Discount discount)
    {
        _context.Discounts.Update(discount);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var discount = _context.Discounts.Find(id);
        if (discount == null) return;
        discount.IsActive = false;
        _context.SaveChanges();
    }
}
