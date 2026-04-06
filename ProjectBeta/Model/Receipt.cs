namespace ProjectBeta.Model;

public class Receipt
{
    public int Id { get; set; }
    public int Booking_ID { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
