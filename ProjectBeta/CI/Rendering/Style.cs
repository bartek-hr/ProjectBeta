namespace ProjectBeta.CI.Rendering;

public sealed record Style
{
    public ConsoleColor? Fg { get; init; }
    public ConsoleColor? Bg { get; init; }
    public bool Bold { get; init; }
    public bool Dim { get; init; }
    public bool Underline { get; init; }
    public bool Strikethrough { get; init; }
    public bool Italic { get; init; }

    public Style WithFg(ConsoleColor c) => this with { Fg = c };
    public Style WithBg(ConsoleColor c) => this with { Bg = c };
    public Style WithBold() => this with { Bold = true };
    public Style WithDim() => this with { Dim = true };
    public Style WithUnderline() => this with { Underline = true };
    public Style WithStrikethrough() => this with { Strikethrough = true };
    public Style WithItalic() => this with { Italic = true };

    public Style Merge(Style parent) => new()
    {
        Fg = Fg ?? parent.Fg,
        Bg = Bg ?? parent.Bg,
        Bold = Bold || parent.Bold,
        Dim = Dim || parent.Dim,
        Underline = Underline || parent.Underline,
        Strikethrough = Strikethrough || parent.Strikethrough,
        Italic = Italic || parent.Italic,
    };

    public static Style Default => new();
    public static Style Error => new() { Fg = ConsoleColor.Red };
    public static Style Muted => new() { Dim = true };
    public static Style Highlight => new() { Fg = ConsoleColor.Black, Bg = ConsoleColor.Cyan };
    public static Style Success => new() { Fg = ConsoleColor.Green };
    public static Style Warning => new() { Fg = ConsoleColor.Yellow };
    public static Style Primary => new() { Fg = ConsoleColor.Cyan, Bold = true };
}
