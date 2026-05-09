using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public class Table : Component
{
    protected readonly List<ColumnDefinition> _columns = [];
    protected readonly List<string[]> _rows = [];
    protected string _emptyMessage = l10n("components.table.no_rows");

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

    protected virtual Style GetBorderStyle() => Style.Muted;

    protected virtual Style GetRowStyle(int rowIndex) => Style.Default;

    public override int Render(ComponentRenderContext context)
    {
        if (_columns.Count == 0)
            return 0;

        var widths = CalculateColumnWidths(context.Width);
        var rowsWritten = 0;
        var buffer = context.Buffer;
        var borderStyle = GetBorderStyle();

        WriteSeparator(buffer, '┌', '┬', '┐', widths, borderStyle);
        rowsWritten++;

        WriteRow(buffer, widths, _columns.Select(column => column.Header).ToArray(), Style.Primary, borderStyle);
        rowsWritten++;

        WriteSeparator(buffer, '├', '┼', '┤', widths, borderStyle);
        rowsWritten++;

        if (_rows.Count == 0)
        {
            WriteRow(buffer, widths, CreateEmptyRow(), Style.Muted, borderStyle);
            rowsWritten++;
        }
        else
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                var rowStyle = GetRowStyle(i);
                WriteRow(buffer, widths, _rows[i], rowStyle, borderStyle);
                rowsWritten++;
            }
        }

        WriteSeparator(buffer, '└', '┴', '┘', widths, borderStyle);
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

    protected static string FormatCell(object? value)
    {
        return value?.ToString()?.Replace("\r", string.Empty).Replace('\n', ' ') ?? string.Empty;
    }

    private static void WriteSeparator(TerminalBuffer buffer, char left, char middle, char right, IReadOnlyList<int> widths, Style borderStyle)
    {
        buffer.Write(left.ToString(), borderStyle);
        for (var index = 0; index < widths.Count; index++)
        {
            buffer.Repeat('─', widths[index] + 2, borderStyle);
            buffer.Write(index == widths.Count - 1 ? right.ToString() : middle.ToString(), borderStyle);
        }

        buffer.WriteLine();
    }

    private void WriteRow(TerminalBuffer buffer, IReadOnlyList<int> widths, IReadOnlyList<string> cells, Style style, Style borderStyle)
    {
        buffer.Write("│", borderStyle);
        for (var index = 0; index < widths.Count; index++)
        {
            var cell = index < cells.Count ? cells[index] : string.Empty;

            buffer.Write(" ", style);
            buffer.WriteFixed(cell, widths[index], _columns[index].Align, style);
            buffer.Write(" ", style);
            buffer.Write("│", borderStyle);
        }

        buffer.WriteLine();
    }

    protected sealed record ColumnDefinition(string Header, Align Align);
}
