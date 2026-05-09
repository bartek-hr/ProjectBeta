# Localization Guide

Locale files live in `ProjectBeta/lang` and use the `.lang` extension. Files may use nested YAML-like maps. The loader flattens them internally, so `messages.welcome` maps to:

```lang
messages:
  welcome: Welcome!
```

## Signatures

- `l10n(string key, string? locale = null)`
- `l10n(string key, IReadOnlyDictionary<string, string> replacements)`
- `l10n(string key, IReadOnlyDictionary<string, string> replacements, string? locale)`

## Locale Files

Pattern: `ProjectBeta/lang/<locale>.lang`

Fallback order:

1. requested locale
2. `en-GB`
3. original key

## Placeholders

Syntax: `:name`

- Replacement keys are matched case-insensitively.
- `:NAME` uppercases the replacement.
- `:Name` capitalizes the replacement.

Example translation:

```lang
welcome: Welcome, :name!
```

Example call:

```csharp
l10n("messages.welcome", new Dictionary<string, string> { ["name"] = "Ruben" })
```

## Pluralization

If a translation contains `|`, plural selection uses the `count` replacement from the dictionary.

Simple:

```lang
car|cars
```

Explicit rules:

```lang
{0}no cars
{1}car
{2,}:count cars
[2,*]:count cars
[1,19]a few cars
```

- `{n}` matches exactly `n`
- `{n,}` matches `n` or more
- `{,n}` matches up to `n`
- `[a,b]` matches the inclusive range `a..b`
- `[a,*]` and `[a,Inf]` match `a` or more

## Examples

```csharp
l10n("app.title")
l10n("app.title", "nl-NL")
l10n("messages.welcome", new Dictionary<string, string> { ["name"] = "Ruben" })
l10n("inventory.car_detailed", new Dictionary<string, string> { ["count"] = "3" })
```
