namespace ProjectBeta.Model;

public class MovieSchedule
{
    public int Id { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public string MovieId { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Movie Movie { get; set; } = null!;
}
