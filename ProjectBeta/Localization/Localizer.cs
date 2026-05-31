using System.Globalization;
using System.Text.RegularExpressions;

namespace ProjectBeta.Localization;

public static class Localizer
{
    private const string DefaultLocale = "en-GB";
    private static readonly object SyncRoot = new();
    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> LocaleCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyDictionary<string, string> EmptyDictionary = new Dictionary<string, string>(StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<string, string> SupportedLocaleNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["en-GB"] = "English",
        ["nl-NL"] = "Nederlands"
    };
    private static readonly Regex PlaceholderPattern = new(@":(?<name>\w+)\b", RegexOptions.Compiled);
    private static string _currentLocale = DefaultLocale;

    public static IReadOnlyDictionary<string, string> SupportedLocales => SupportedLocaleNames;

    public static string l10n(string key, string? locale = null)
    {
        return Translate(key, null, locale);
    }

    public static string l10n(string key, IReadOnlyDictionary<string, string> replacements)
    {
        return Translate(key, replacements, null);
    }

    public static string l10n(string key, IReadOnlyDictionary<string, string> replacements, string? locale)
    {
        return Translate(key, replacements, locale);
    }

    public static void SetLocale(string locale)
    {
        lock (SyncRoot)
        {
            _currentLocale = NormalizeStoredLocale(locale);
        }
    }

    public static string GetLocale()
    {
        return GetCurrentLocale();
    }

    private static string Translate(string key, IReadOnlyDictionary<string, string>? replacements, string? locale)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return key;
        }

        var requestedLocale = NormalizeRequestedLocale(locale) ?? GetCurrentLocale();

        foreach (var candidateLocale in GetLookupLocales(requestedLocale))
        {
            if (GetTranslations(candidateLocale).TryGetValue(key, out var value))
            {
                return FormatTranslation(value, replacements);
            }
        }

        return key;
    }

    private static string GetCurrentLocale()
    {
        lock (SyncRoot)
        {
            return _currentLocale;
        }
    }

    private static IEnumerable<string> GetLookupLocales(string locale)
    {
        yield return locale;

        if (!string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase))
        {
            yield return DefaultLocale;
        }
    }

    private static IReadOnlyDictionary<string, string> GetTranslations(string locale)
    {
        lock (SyncRoot)
        {
            if (!LocaleCache.TryGetValue(locale, out var translations))
            {
                translations = LoadTranslations(locale);
                LocaleCache[locale] = translations;
            }

            return translations;
        }
    }

    private static IReadOnlyDictionary<string, string> LoadTranslations(string locale)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "lang", $"{locale}.lang");
        if (!File.Exists(path))
        {
            return EmptyTranslations();
        }

        try
        {
            return ParseTranslations(File.ReadLines(path));
        }
        catch
        {
            return EmptyTranslations();
        }
    }

    private static IReadOnlyDictionary<string, string> ParseTranslations(IEnumerable<string> lines)
    {
        var translations = new Dictionary<string, string>(StringComparer.Ordinal);
        var keyStack = new Stack<(int Indent, string Key)>();

        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var line = rawLine.TrimEnd();
            var trimmed = line.TrimStart(' ', '\t');
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var indent = line.Length - trimmed.Length;
            var separatorIndex = trimmed.IndexOf(':');
            if (separatorIndex <= 0)
            {
                throw new FormatException("Invalid locale line.");
            }

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();
            if (key.Length == 0)
            {
                throw new FormatException("Empty locale key.");
            }

            while (keyStack.Count > 0 && indent <= keyStack.Peek().Indent)
            {
                keyStack.Pop();
            }

            if (value.Length == 0)
            {
                keyStack.Push((indent, key));
                continue;
            }

            var fullKeyParts = keyStack.Reverse().Select(entry => entry.Key).Append(key);
            translations[string.Join('.', fullKeyParts)] = Unquote(value);
        }

        return translations;
    }

    private static IReadOnlyDictionary<string, string> EmptyTranslations()
    {
        return EmptyDictionary;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2)
        {
            var first = value[0];
            var last = value[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
            {
                return value[1..^1];
            }
        }

        return value;
    }

    private static string? NormalizeRequestedLocale(string? locale)
    {
        return string.IsNullOrWhiteSpace(locale) ? null : locale.Trim();
    }

    private static string NormalizeStoredLocale(string locale)
    {
        return string.IsNullOrWhiteSpace(locale) ? DefaultLocale : locale.Trim();
    }

    private static string FormatTranslation(string value, IReadOnlyDictionary<string, string>? replacements)
    {
        var normalizedReplacements = NormalizeReplacements(replacements);
        var selectedValue = SelectPluralForm(value, normalizedReplacements);
        return ReplacePlaceholders(selectedValue, normalizedReplacements);
    }

    private static IReadOnlyDictionary<string, string> NormalizeReplacements(IReadOnlyDictionary<string, string>? replacements)
    {
        if (replacements is null || replacements.Count == 0)
        {
            return EmptyDictionary;
        }

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in replacements)
        {
            normalized[pair.Key] = pair.Value;
        }

        return normalized;
    }

    private static string SelectPluralForm(string value, IReadOnlyDictionary<string, string> replacements)
    {
        if (!value.Contains('|'))
        {
            return value;
        }

        var segments = value
            .Split('|', StringSplitOptions.TrimEntries)
            .Select(ParsePluralSegment)
            .ToArray();

        if (segments.Length == 0)
        {
            return value;
        }

        if (!TryGetCount(replacements, out var count))
        {
            return segments[0].Text;
        }

        var hasExplicitSelectors = segments.Any(segment => segment.Matcher is not null);
        if (hasExplicitSelectors)
        {
            foreach (var segment in segments)
            {
                if (segment.Matcher?.Invoke(count) == true)
                {
                    return segment.Text;
                }
            }

            return segments[^1].Text;
        }

        if (segments.Length == 2)
        {
            return count == 1 ? segments[0].Text : segments[1].Text;
        }

        return count == 1 ? segments[0].Text : segments[^1].Text;
    }

    private static bool TryGetCount(IReadOnlyDictionary<string, string> replacements, out decimal count)
    {
        count = 0;
        if (!replacements.TryGetValue("count", out var rawCount))
        {
            return false;
        }

        return decimal.TryParse(rawCount, NumberStyles.Number, CultureInfo.InvariantCulture, out count);
    }

    private static PluralSegment ParsePluralSegment(string segment)
    {
        var trimmed = segment.Trim();
        if (trimmed.Length == 0)
        {
            return new PluralSegment(null, string.Empty);
        }

        if (TryParseBraceSelector(trimmed, out var braceLength, out var braceMatcher))
        {
            return new PluralSegment(braceMatcher, trimmed[braceLength..].TrimStart());
        }

        if (TryParseBracketSelector(trimmed, out var bracketLength, out var bracketMatcher))
        {
            return new PluralSegment(bracketMatcher, trimmed[bracketLength..].TrimStart());
        }

        return new PluralSegment(null, trimmed);
    }

    private static bool TryParseBraceSelector(string segment, out int selectorLength, out Func<decimal, bool>? matcher)
    {
        selectorLength = 0;
        matcher = null;
        if (!segment.StartsWith('{'))
        {
            return false;
        }

        var closingIndex = segment.IndexOf('}');
        if (closingIndex <= 1)
        {
            return false;
        }

        var selector = segment[1..closingIndex].Trim();
        selectorLength = closingIndex + 1;

        if (TryParseNumber(selector, out var exactValue))
        {
            matcher = count => count == exactValue;
            return true;
        }

        if (!selector.Contains(','))
        {
            return false;
        }

        var bounds = selector.Split(',', 2);
        if (!TryParseBound(bounds[0], out var min, allowWildcard: true) ||
            !TryParseBound(bounds[1], out var max, allowWildcard: true))
        {
            return false;
        }

        matcher = count => IsWithinBounds(count, min, max);
        return true;
    }

    private static bool TryParseBracketSelector(string segment, out int selectorLength, out Func<decimal, bool>? matcher)
    {
        selectorLength = 0;
        matcher = null;
        if (!segment.StartsWith('['))
        {
            return false;
        }

        var closingIndex = segment.IndexOf(']');
        if (closingIndex <= 1)
        {
            return false;
        }

        var selector = segment[1..closingIndex].Trim();
        var bounds = selector.Split(',', 2);
        if (bounds.Length != 2)
        {
            return false;
        }

        if (!TryParseBound(bounds[0], out var min, allowWildcard: false) ||
            !TryParseBound(bounds[1], out var max, allowWildcard: true))
        {
            return false;
        }

        selectorLength = closingIndex + 1;
        matcher = count => IsWithinBounds(count, min, max);
        return true;
    }

    private static bool TryParseBound(string rawValue, out decimal? bound, bool allowWildcard)
    {
        var value = rawValue.Trim();
        if (value.Length == 0)
        {
            bound = null;
            return true;
        }

        if (allowWildcard && (value == "*" || value.Equals("inf", StringComparison.OrdinalIgnoreCase) || value.Equals("infinity", StringComparison.OrdinalIgnoreCase)))
        {
            bound = null;
            return true;
        }

        if (TryParseNumber(value, out var number))
        {
            bound = number;
            return true;
        }

        bound = null;
        return false;
    }

    private static bool TryParseNumber(string value, out decimal number)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out number);
    }

    private static bool IsWithinBounds(decimal count, decimal? min, decimal? max)
    {
        return (!min.HasValue || count >= min.Value) &&
               (!max.HasValue || count <= max.Value);
    }

    private static string ReplacePlaceholders(string value, IReadOnlyDictionary<string, string> replacements)
    {
        if (replacements.Count == 0 || !value.Contains(':'))
        {
            return value;
        }

        return PlaceholderPattern.Replace(value, match =>
        {
            var token = match.Groups["name"].Value;
            if (!replacements.TryGetValue(token, out var replacement))
            {
                return match.Value;
            }

            return ApplyPlaceholderCasing(token, replacement);
        });
    }

    private static string ApplyPlaceholderCasing(string token, string replacement)
    {
        if (token.All(char.IsUpper))
        {
            return replacement.ToUpperInvariant();
        }

        if (char.IsUpper(token[0]) && token[1..].All(char.IsLower))
        {
            return Capitalize(replacement);
        }

        return replacement;
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return value.ToUpperInvariant();
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private readonly record struct PluralSegment(Func<decimal, bool>? Matcher, string Text);
}
