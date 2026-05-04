namespace ProjectBeta.Model;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ScreeningId { get; set; }
    // TODO: add below when Screening model is created
    // public Screening Screening {get; set;} = null!;
    public string Seats{get;set;}
    public int? DiscountId { get; set; }
    public int? AuditoriumId { get; set; }
    public string? Movie { get; set; }
    // TODO: add below when Discount model is created
    // public Discount Discount {get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public bool Paid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
