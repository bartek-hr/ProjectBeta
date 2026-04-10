using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Table : Component
{
    private readonly List<ColumnDefinition> _columns = [];
    private readonly List<string[]> _rows = [];
    private string _emptyMessage = "No rows";

    public Table(params string[] headers)
    {
        foreach (var header in headers)
            AddColumn(header);
    }

    public Table AddColumn(string header, Align align = Align.Left)
    {
        _columns.Add(new ColumnDefinition(header, align));
        return this;
    }

    public Table AddRow(params object?[] cells)
    {
        if (_columns.Count == 0)
            throw new InvalidOperationException("Add at least one column before adding rows.");

        var row = new string[_columns.Count];
        for (var index = 0; index < _columns.Count; index++)
        {
            row[index] = index < cells.Length
                ? FormatCell(cells[index])
                : string.Empty;
        }

        _rows.Add(row);
        return this;
    }

    public Table EmptyMessage(string message)
    {
        _emptyMessage = message;
        return this;
    }

    public override int Render(ComponentRenderContext context)
    {
        if (_columns.Count == 0)
            return 0;

        var widths = CalculateColumnWidths(context.Width);
        var rowsWritten = 0;
        var buffer = context.Buffer;

        WriteSeparator(buffer, '┌', '┬', '┐', widths);
        rowsWritten++;

        WriteRow(buffer, widths, _columns.Select(column => column.Header).ToArray(), Style.Primary);
        rowsWritten++;

        WriteSeparator(buffer, '├', '┼', '┤', widths);
        rowsWritten++;

        if (_rows.Count == 0)
        {
            WriteRow(buffer, widths, CreateEmptyRow(), Style.Muted);
            rowsWritten++;
        }
        else
        {
            foreach (var row in _rows)
            {
                WriteRow(buffer, widths, row, Style.Default);
                rowsWritten++;
            }
        }

        WriteSeparator(buffer, '└', '┴', '┘', widths);
        rowsWritten++;

        return rowsWritten;
    }

    private int[] CalculateColumnWidths(int availableWidth)
    {
        var widths = _columns
            .Select((column, index) => MeasureColumnWidth(column.Header, index))
            .ToArray();

        var fixedWidth = (_columns.Count * 3) + 1;
        var maxContentWidth = Math.Max(_columns.Count, availableWidth - fixedWidth);
        var currentContentWidth = widths.Sum();

        while (currentContentWidth > maxContentWidth)
        {
            var widestColumn = 0;
            for (var index = 1; index < widths.Length; index++)
            {
                if (widths[index] > widths[widestColumn])
                    widestColumn = index;
            }

            if (widths[widestColumn] <= 1)
                break;

            widths[widestColumn]--;
            currentContentWidth--;
        }

        return widths;
    }

    private int MeasureColumnWidth(string header, int columnIndex)
    {
        var width = header.Length;
        foreach (var row in _rows)
        {
            if (columnIndex < row.Length)
                width = Math.Max(width, row[columnIndex].Length);
        }

        return Math.Max(1, width);
    }

    private string[] CreateEmptyRow()
    {
        var row = Enumerable.Repeat(string.Empty, _columns.Count).ToArray();
        row[0] = _emptyMessage;
        return row;
    }

    private static string FormatCell(object? value)
    {
        return value?.ToString()?.Replace("\r", string.Empty).Replace('\n', ' ') ?? string.Empty;
    }

    private static void WriteSeparator(TerminalBuffer buffer, char left, char middle, char right, IReadOnlyList<int> widths)
    {
        buffer.Write(left.ToString(), Style.Muted);
        for (var index = 0; index < widths.Count; index++)
        {
            buffer.Repeat('─', widths[index] + 2, Style.Muted);
            buffer.Write(index == widths.Count - 1 ? right.ToString() : middle.ToString(), Style.Muted);
        }

        buffer.WriteLine();
    }

    private void WriteRow(TerminalBuffer buffer, IReadOnlyList<int> widths, IReadOnlyList<string> cells, Style style)
    {
        buffer.Write("│", Style.Muted);
        for (var index = 0; index < widths.Count; index++)
        {
            var cell = index < cells.Count ? cells[index] : string.Empty;

            buffer.Write(" ", style);
            buffer.WriteFixed(cell, widths[index], _columns[index].Align, style);
            buffer.Write(" ", style);
            buffer.Write("│", Style.Muted);
        }

        buffer.WriteLine();
    }

    private sealed record ColumnDefinition(string Header, Align Align);
}
