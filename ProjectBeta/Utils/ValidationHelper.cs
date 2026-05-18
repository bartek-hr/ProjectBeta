namespace ProjectBeta.Utils;

public static class ValidationHelper
{
    public static bool AnyNullOrWhiteSpace(params string[] values)
    {
        return values.Any(string.IsNullOrWhiteSpace);
    }
}
