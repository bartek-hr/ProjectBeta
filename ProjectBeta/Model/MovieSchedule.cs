namespace ProjectBeta.Model;

public class MovieSchedule
{
    public int Id { get; set; }
    public DateOnly ScheduleDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string MovieId { get; set; } = string.Empty;
    public Movie Movie { get; set; } = null!;
}
