namespace ProjectBeta.Model;

public class BookingSnack
{
    public int Id { get; set; }
    public int SnackId { get; set; }
    public Snack Snack { get; set; } = null!;
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int BookedQuantity { get; set; }
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
