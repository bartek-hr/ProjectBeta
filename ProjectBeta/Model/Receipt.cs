namespace ProjectBeta.Model;

public class Receipt
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking {get; set; } = null!;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
