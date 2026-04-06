namespace ProjectBeta.Model;

public class Booking
{
    public int Id { get; set; }
    public int User_Id { get; set; }
    public int Screening_ID { get; set; }
    public int? Discount_ID { get; set; }
    public decimal Total_Price { get; set; }
    public bool Paid { get; set; }
    public DateTime CreatedAt { get; set; }
}
