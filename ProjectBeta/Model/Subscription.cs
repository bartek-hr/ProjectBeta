using System.ComponentModel.DataAnnotations;

namespace ProjectBeta.Model
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        [Range(0, 6)]
        public int? ApplicableDayOfWeek { get; set; }
        public int SeatPriceId { get; set; }
        public SeatPrice SeatPrice { get; set; }
        public bool IsConnectAllowed { get; set; }
        public decimal ConnectDiscount { get; set; }
        public ICollection<UserSubscription> UserSubscriptions { get; set; }
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveUntil { get; set; }
    }
}