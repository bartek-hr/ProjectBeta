using System.Collections.Generic;
using System.Linq;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access
{
    public class SubscriptionAccess
    {
        private readonly AppDbContext _context;
        public SubscriptionAccess(AppDbContext context)
        {
            _context = context;
        }

        public List<Subscription> GetActiveSubscriptions()
        {
            return _context.Subscriptions.Where(s => s.IsActive).ToList();
        }

        public Subscription? GetSubscriptionById(int id)
        {
            return _context.Subscriptions.FirstOrDefault(s => s.Id == id);
        }

        public void AddSubscription(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();
        }

        public void RemoveSubscription(int id)
        {
            var sub = _context.Subscriptions.FirstOrDefault(s => s.Id == id);
            if (sub != null)
            {
                sub.IsActive = false;
                _context.SaveChanges();
            }
        }
    }
}