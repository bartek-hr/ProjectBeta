using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class ReceiptAccess
    {
        private readonly AppDbContext _context;

        public ReceiptAccess(AppDbContext context)
        {
            _context = context;
        }

        public List<Receipt> GetAll()
        {
            return _context.Receipts.ToList();
        }

        public Receipt? GetById(int id)
        {
            return _context.Receipts.FirstOrDefault(b => b.Id == id);
        }

        public void Add(Receipt Receipt)
        {
            _context.Receipts.Add(Receipt);
            _context.SaveChanges();
        }

        public void Update(Receipt Receipt)
        {
            _context.Receipts.Update(Receipt);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var Receipt = _context.Receipts.Find(id);
            if (Receipt != null)
            {
                _context.Receipts.Remove(Receipt);
                _context.SaveChanges();
            }
        }
    }
