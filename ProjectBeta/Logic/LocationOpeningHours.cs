namespace ProjectBeta.Logic;

public sealed record LocationOpeningHours(TimeOnly? OpeningTime, TimeOnly? ClosingTime)
{
    public bool IsClosed => OpeningTime == null && ClosingTime == null;
}
