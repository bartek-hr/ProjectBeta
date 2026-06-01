namespace ProjectBeta.Model;

public class LocationOpeningTime
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
