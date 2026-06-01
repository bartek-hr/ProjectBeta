namespace ProjectBeta.CI.Rendering;

public sealed class TerminalFrame
{
    public TerminalFrame(IReadOnlyList<List<Span>> rows, int width, int? cursorLeft, int? cursorTop)
    {
        Rows = rows;
        Width = width;
        CursorLeft = cursorLeft;
        CursorTop = cursorTop;
    }

    public IReadOnlyList<List<Span>> Rows { get; }
    public int Width { get; }
    public int Height => Rows.Count;
    public int? CursorLeft { get; }
    public int? CursorTop { get; }

    public string GetRowPlainText(int row)
    {
        if (row < 0 || row >= Rows.Count)
            return string.Empty;

        var spans = Rows[row];
        if (spans.Count == 0)
            return string.Empty;

        var length = 0;
        foreach (var span in spans)
            length += span.Length;

        return string.Create(length, spans, static (dest, spans) =>
        {
            var pos = 0;
            foreach (var span in spans)
            {
                span.Text.AsSpan().CopyTo(dest[pos..]);
                pos += span.Length;
            }
        });
    }

    /// <summary>
    /// Returns per-character styled cells for a row, used for frame diff comparison.
    /// Each cell is a (char, Style) pair so style-only changes are detected.
    /// </summary>
    public StyledCell[] GetRowStyledCells(int row)
    {
        if (row < 0 || row >= Rows.Count)
            return [];

        var spans = Rows[row];
        if (spans.Count == 0)
            return [];

        var totalLength = 0;
        foreach (var span in spans)
            totalLength += span.Length;

        var cells = new StyledCell[totalLength];
        var pos = 0;
        foreach (var span in spans)
        {
            for (var i = 0; i < span.Text.Length; i++)
            {
                cells[pos++] = new StyledCell(span.Text[i], span.Style);
            }
        }

        return cells;
    }
}

public readonly record struct StyledCell(char Value, Style Style);
