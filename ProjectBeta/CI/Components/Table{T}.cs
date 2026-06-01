using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Table<T> : Table where T : class
{
    private readonly List<T> _rowData = [];
    private int _highlightedIndex;
    private Action<T>? _onSelect;

    public Table(params string[] headers) : base(headers)
    {
    }

    public override bool IsFocusable => _rows.Count > 0 && _onSelect != null;

    public new Table<T> AddColumn(string header, Align align = Align.Left)
    {
        base.AddColumn(header, align);
        return this;
    }

    public Table<T> AddRow(T data, params object?[] cells)
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
        _rowData.Add(data);
        return this;
    }

    public new Table<T> EmptyMessage(string message)
    {
        _emptyMessage = message;
        return this;
    }

    public Table<T> OnSelect(Action<T> callback)
    {
        _onSelect = callback;
        return this;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (_rows.Count == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _highlightedIndex = (_highlightedIndex - 1 + _rows.Count) % _rows.Count;
                return true;
            case ConsoleKey.DownArrow:
                _highlightedIndex = (_highlightedIndex + 1) % _rows.Count;
                return true;
            case ConsoleKey.Enter or ConsoleKey.Spacebar:
                if (_highlightedIndex >= 0 && _highlightedIndex < _rowData.Count)
                    _onSelect?.Invoke(_rowData[_highlightedIndex]);
                return true;
            default:
                return false;
        }
    }

    protected override Style GetBorderStyle() => IsFocused ? Style.Primary : Style.Muted;

    protected override Style GetRowStyle(int rowIndex)
    {
        if (_rows.Count > 0)
            _highlightedIndex = Math.Clamp(_highlightedIndex, 0, _rows.Count - 1);

        return IsFocused && rowIndex == _highlightedIndex ? Style.Highlight : Style.Default;
    }
}
