using System.Collections.Generic;
using System.Linq;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access
{
    public class UserSubscriptionAccess
    {
        private readonly AppDbContext _context;
        public UserSubscriptionAccess(AppDbContext context)
        {
            _context = context;
        }

        public UserSubscription? GetActiveByUserId(int userId)
        {
            return _context.UserSubscriptions.FirstOrDefault(us => us.UserId == userId && us.IsActive);
        }

        public List<UserSubscription> GetAllByUserId(int userId)
        {
            return _context.UserSubscriptions.Where(us => us.UserId == userId).ToList();
        }
    }
}