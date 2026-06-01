using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

/// <summary>
/// Shared box-drawing helpers for input components.
/// Renders:
///   ┌── Label ─────────────────────────┐
///   │ content                          │
///   └──────────────────────────────────┘
///   error message (or blank reserved)
/// </summary>
internal static class InputBox
{
    private const int Margin = 2;
    private const int MinBoxWidth = 20;
    private const int MaxBoxWidth = 60;

    public static Style GetBorderStyle(bool focused, bool hasErrors)
    {
        if (focused) return Style.Primary;
        if (hasErrors) return Style.Error;
        return Style.Muted;
    }

    public static int GetBoxWidth(int availableWidth)
    {
        return Math.Clamp(availableWidth - Margin, MinBoxWidth, MaxBoxWidth);
    }

    /// <summary>
    /// The number of visible characters that fit inside the content area of the box.
    /// Layout: "  │ " (4 chars) + content + " │" (2 chars) — but WriteFixedContentRow
    /// uses innerWidth = boxWidth - 4, then appends " │". So content area = boxWidth - 4.
    /// </summary>
    public static int GetInnerWidth(int boxWidth)
    {
        return Math.Max(1, boxWidth - 4);
    }

    /// <summary>
    /// Draws the top border:  ┌── Label ────────────────┐
    /// </summary>
    public static void WriteTopBorder(TerminalBuffer buf, string label, int boxWidth, Style borderStyle)
    {
        buf.Write("  ");
        buf.Write("\u250c\u2500\u2500 ", borderStyle);
        buf.Write(label, borderStyle.Fg != null
            ? new Style { Fg = borderStyle.Fg, Bold = true }
            : Style.Default.WithBold());
        buf.Write(" ", borderStyle);

        var used = 4 + label.Length + 1;
        var remaining = boxWidth - used - 1;
        if (remaining > 0)
            buf.Repeat('\u2500', remaining, borderStyle);

        buf.Write("\u2510", borderStyle);
        buf.WriteLine();
    }

    /// <summary>
    /// Draws a content row:   │ content                 │
    /// Returns the absolute column where inner content starts (for cursor positioning).
    /// </summary>
    public static int WriteContentRow(TerminalBuffer buf, int boxWidth, Style borderStyle,
        Action<TerminalBuffer> writeContent)
    {
        buf.Write("  ");
        buf.Write("\u2502 ", borderStyle);

        var contentStart = 4; // "  │ " = 4 chars
        writeContent(buf);

        // We need to figure out how many visible chars were written inside the content action.
        // Pad to fill the box, then close with │
        // The simplest approach: we know the box interior is (boxWidth - 2) chars wide ("│ " + " │" = 2+2 but content area = boxWidth - 4 for border+space)
        // Actually: │(space)(content padded to innerWidth)(space)│  -- no, simpler:
        // "│ " + content + pad + "│"  where total = boxWidth
        // inner area = boxWidth - 3  (for "│ " at start and "│" at end)
        // So content + pad = boxWidth - 3

        // We can't easily measure what writeContent wrote.
        // Instead, let's not pad here — we'll use WriteFixedContentRow for the common case.
        return contentStart;
    }

    /// <summary>
    /// Draws a content row with a fixed-width text value, properly padded.
    /// </summary>
    public static void WriteFixedContentRow(TerminalBuffer buf, int boxWidth, Style borderStyle,
        string text, Style textStyle)
    {
        buf.Write("  ");
        buf.Write("\u2502 ", borderStyle);

        var innerWidth = boxWidth - 4; // "│ " ... " │"
        if (text.Length > innerWidth)
            text = text[..innerWidth];

        buf.Write(text, textStyle);

        var padding = innerWidth - text.Length;
        if (padding > 0)
            buf.Repeat(' ', padding);

        buf.Write(" \u2502", borderStyle);
        buf.WriteLine();
    }

    /// <summary>
    /// Draws the bottom border: └────────────────────────┘
    /// </summary>
    public static void WriteBottomBorder(TerminalBuffer buf, int boxWidth, Style borderStyle)
    {
        buf.Write("  ");
        buf.Write("\u2514", borderStyle);

        var fillWidth = boxWidth - 2;
        if (fillWidth > 0)
            buf.Repeat('\u2500', fillWidth, borderStyle);

        buf.Write("\u2518", borderStyle);
        buf.WriteLine();
    }

    /// <summary>
    /// Writes the error line (always 1 line — blank if no errors, to prevent UI jumping).
    /// </summary>
    public static void WriteErrorLine(TerminalBuffer buf, List<string> errors)
    {
        if (errors.Count > 0)
        {
            buf.Write("    ").WriteLine(errors[0], Style.Error);
        }
        else
        {
            buf.WriteLine();
        }
    }

    /// <summary>
    /// Total lines rendered by a standard input box (top + content + bottom + error).
    /// </summary>
    public const int StandardLineCount = 4;
}
