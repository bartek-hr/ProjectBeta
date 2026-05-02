using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ProjectBeta.Utils;

public static class ValidationHelper
{
    public static bool AnyNullOrWhiteSpace(params string[] values)
    {
        return values.Any(x => string.IsNullOrEmpty(x));
    }
}