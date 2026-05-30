using System;

namespace ProjectBeta.Model
{
    public class UserSubscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsConnected { get; set; }
        public int? ConnectedWithUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }


    }
}