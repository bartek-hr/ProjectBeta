namespace ProjectBeta.CI.Rendering;

public sealed class TerminalBuffer
{
    private readonly List<List<Span>> _lines = [];
    private List<Span> _currentLine = [];
    private readonly Stack<int> _indentStack = new();

    public TerminalBuffer(int width)
    {
        Width = Math.Max(1, width);
    }

    public int Width { get; }
    public int Height => _lines.Count + (_currentLine.Count > 0 ? 1 : 0);
    public int? CursorLeft { get; private set; }
    public int? CursorTop { get; private set; }

    public TerminalBuffer Write(string? text, Style? style = null)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        _currentLine.Add(new Span(text, style ?? Style.Default));
        return this;
    }

    public TerminalBuffer WriteLine(string? text = null, Style? style = null)
    {
        if (!string.IsNullOrEmpty(text))
            _currentLine.Add(new Span(text, style ?? Style.Default));

        FinishLine();
        return this;
    }

    public TerminalBuffer BlankLine(int count = 1)
    {
        FinishCurrentIfNeeded();
        for (var i = 0; i < count; i++)
            _lines.Add([]);

        return this;
    }

    public TerminalBuffer Repeat(char ch, int count, Style? style = null)
    {
        if (count <= 0)
            return this;

        _currentLine.Add(new Span(new string(ch, count), style ?? Style.Default));
        return this;
    }

    public TerminalBuffer WriteFixed(string text, int width, Align align = Align.Left, Style? style = null)
    {
        if (width <= 0)
            return this;

        string fitted;
        if (text.Length > width)
        {
            fitted = text[..width];
        }
        else
        {
            fitted = align switch
            {
                Align.Right => text.PadLeft(width),
                Align.Center => text.PadLeft((width + text.Length) / 2).PadRight(width),
                _ => text.PadRight(width),
            };
        }

        _currentLine.Add(new Span(fitted, style ?? Style.Default));
        return this;
    }

    public TerminalBuffer WriteIf(bool condition, string text, Style? style = null)
    {
        if (condition)
            Write(text, style);

        return this;
    }

    public TerminalBuffer WriteIf(bool condition, Action<TerminalBuffer> ifTrue, Action<TerminalBuffer>? ifFalse = null)
    {
        if (condition)
            ifTrue(this);
        else
            ifFalse?.Invoke(this);

        return this;
    }

    public TerminalBuffer WriteJoin(string separator, IEnumerable<string> items, Style? style = null)
    {
        var first = true;
        foreach (var item in items)
        {
            if (!first)
                Write(separator, style);
            Write(item, style);
            first = false;
        }

        return this;
    }

    public TerminalBuffer Pad(int left)
    {
        if (left > 0)
            _currentLine.Insert(0, new Span(new string(' ', left)));

        return this;
    }

    public IDisposable Indent(int spaces = 2)
    {
        _indentStack.Push(spaces);
        return new IndentScope(this);
    }

    public TerminalBuffer SetCursor(int x, int y)
    {
        CursorLeft = Math.Clamp(x, 0, Width - 1);
        CursorTop = Math.Max(0, y);
        return this;
    }

    public TerminalBuffer SetCursorHere()
    {
        var x = CurrentLineLength();
        var y = _lines.Count;
        return SetCursor(x, y);
    }

    public TerminalBuffer Append(TerminalBuffer other)
    {
        var builtLines = other.Build();
        foreach (var line in builtLines)
        {
            _lines.Add(new List<Span>(line));
        }

        if (other.CursorLeft != null && other.CursorTop != null)
        {
            var offsetY = _lines.Count - builtLines.Count;
            SetCursor(other.CursorLeft.Value, offsetY + other.CursorTop.Value);
        }

        return this;
    }

    public TerminalBuffer Clear()
    {
        _lines.Clear();
        _currentLine.Clear();
        CursorLeft = null;
        CursorTop = null;
        return this;
    }

    public List<List<Span>> Build()
    {
        FinishCurrentIfNeeded();
        var result = new List<List<Span>>(_lines.Count);
        foreach (var line in _lines)
            result.Add(new List<Span>(line));

        return result;
    }

    public TerminalFrame ToFrame()
    {
        var built = Build();
        return new TerminalFrame(built, Width, CursorLeft, CursorTop);
    }

    public void EnsureHeight(int height)
    {
        FinishCurrentIfNeeded();
        while (_lines.Count < height)
            _lines.Add([]);
    }

    private int IndentTotal()
    {
        var total = 0;
        foreach (var indent in _indentStack)
            total += indent;
        return total;
    }

    private void FinishLine()
    {
        var indent = IndentTotal();
        var line = new List<Span>();
        if (indent > 0)
            line.Add(new Span(new string(' ', indent)));
        line.AddRange(_currentLine);
        _lines.Add(line);
        _currentLine = [];
    }

    private void FinishCurrentIfNeeded()
    {
        if (_currentLine.Count > 0)
            FinishLine();
    }

    private int CurrentLineLength()
    {
        var len = 0;
        foreach (var span in _currentLine)
            len += span.Length;
        return len + IndentTotal();
    }

    private sealed class IndentScope(TerminalBuffer buf) : IDisposable
    {
        public void Dispose() => buf._indentStack.Pop();
    }
}

public enum Align
{
    Left,
    Center,
    Right,
}
