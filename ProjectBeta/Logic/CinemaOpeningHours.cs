namespace ProjectBeta.Logic;

public sealed record CinemaOpeningHours(TimeOnly? OpeningTime, TimeOnly? ClosingTime)
{
    public bool IsClosed => OpeningTime == null && ClosingTime == null;
}
