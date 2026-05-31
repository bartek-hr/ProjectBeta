namespace ProjectBeta.Model;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ScreeningId { get; set; }
    public string Seats{get;set;}
    // Comma-separated ages per seat
    public string? SeatAges { get; set; }
    public int? AuditoriumId { get; set; }
    public Auditorium? Auditorium { get; set; }
    public string? Movie { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalPrice { get; set; }
    // (optional, for future subscription use).
    public string? UserSeat { get; set; }
    public bool Paid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<BookingDiscount> BookingDiscounts { get; set; } = new List<BookingDiscount>();
}
