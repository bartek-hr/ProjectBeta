namespace ProjectBeta.Model;

public class BookingDiscount
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int DiscountId { get; set; }
    public Discount Discount { get; set; } = null!;
}
