namespace ProjectBeta.Model;

public class CinemaOpeningTime
{
    public int Id { get; set; }
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
