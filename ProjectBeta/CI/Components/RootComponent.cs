using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public abstract class RootComponent : StaticTerminalInterface
{
    private readonly List<Component> _children = [];
    private readonly IConsoleDriver _consoleDriver;
    private TerminalFrame? _previousFrame;
    private int _previousScrollOffset = -1;
    private bool _closed;
    private Component? _focusedChild;
    private int _scrollOffset;
    private readonly List<(int Start, int End)> _componentRowRanges = [];

    protected RootComponent(IConsoleDriver? consoleDriver = null)
    {
        _consoleDriver = consoleDriver ?? new SystemConsoleDriver();
    }

    public IReadOnlyList<Component> Children => _children;
    public bool IsClosed => _closed;
    internal event Action<RootComponent>? Closed;

    public RootComponent Add(Component child)
    {
        _children.Add(child);

        // Set initial focus to first visible focusable child
        if (_focusedChild == null && child.IsFocusable && !child.IsHidden)
        {
            _focusedChild = child;
            child.IsFocused = true;
        }

        return this;
    }

    public RootComponent AddRange(IEnumerable<Component> children)
    {
        foreach (var child in children)
            Add(child);

        return this;
    }

    public RootComponent AddRange(params Component[] children)
    {
        return AddRange(children.AsEnumerable());
    }

    protected bool RemoveChild(Component child)
    {
        var removed = _children.Remove(child);
        if (!removed)
            return false;

        if (ReferenceEquals(_focusedChild, child))
        {
            _focusedChild.IsFocused = false;
            _focusedChild = null;
            EnsureFocusValid();
        }

        child.IsFocused = false;
        return true;
    }

    public void ClearChildren()
    {
        _children.Clear();
        _focusedChild = null;
    }

    public void Close()
    {
        if (_closed)
            return;

        _closed = true;
        _consoleDriver.CursorVisible = false;
        Closed?.Invoke(this);
    }

    internal void PrepareForDisplay()
    {
        _closed = false;
        Invalidate();
    }

    public override void Invalidate()
    {
        _previousFrame = null;
        _previousScrollOffset = -1;
        _consoleDriver.Clear();
    }

    public override void Render()
    {
        var width = Math.Max(1, _consoleDriver.WindowWidth);
        var viewportHeight = Math.Max(1, _consoleDriver.WindowHeight);
        var buffer = new TerminalBuffer(width);
        var top = 0;

        // If focused child became hidden, move focus to next visible
        EnsureFocusValid();

        // Build full virtual frame, skipping hidden components
        _componentRowRanges.Clear();
        foreach (var child in _children)
        {
            if (child.IsHidden)
            {
                child.IsFocused = false;
                _componentRowRanges.Add((top, top));
                continue;
            }

            var startRow = top;
            var rowsUsed = Math.Max(0, child.Render(new ComponentRenderContext(buffer, top, width)));
            top += rowsUsed;
            buffer.EnsureHeight(top);
            _componentRowRanges.Add((startRow, top));
        }

        var nextFrame = buffer.ToFrame();

        // Auto-scroll to keep focused component visible
        EnsureFocusedVisible(viewportHeight);

        // Clamp scroll offset
        var maxScroll = Math.Max(0, nextFrame.Height - viewportHeight);
        _scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);

        // Determine if we need a full render
        var forceFullRender = _previousFrame == null
                              || _previousFrame.Width != nextFrame.Width
                              || _previousScrollOffset != _scrollOffset;

        if (forceFullRender)
            _consoleDriver.Clear();

        ApplyViewportDiff(_previousFrame, nextFrame, viewportHeight, forceFullRender);
        ApplyViewportCursor(nextFrame, viewportHeight);

        _previousFrame = nextFrame;
        _previousScrollOffset = _scrollOffset;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (_closed)
            return false;

        if (key.Key == ConsoleKey.Escape)
        {
            Close();
            return false;
        }

        if (key.Key == ConsoleKey.PageUp)
        {
            var pageSize = Math.Max(1, _consoleDriver.WindowHeight - 2);
            _scrollOffset = Math.Max(0, _scrollOffset - pageSize);
            return true;
        }

        if (key.Key == ConsoleKey.PageDown)
        {
            var pageSize = Math.Max(1, _consoleDriver.WindowHeight - 2);
            _scrollOffset += pageSize;
            return true;
        }

        if (key.Key == ConsoleKey.Tab)
        {
            var direction = key.Modifiers.HasFlag(ConsoleModifiers.Shift) ? -1 : 1;
            return MoveFocus(direction);
        }

        return _focusedChild is { IsHidden: false }
            ? _focusedChild.ProcessKey(key)
            : false;
    }

    public void FocusChild(Component child)
    {
        if (!_children.Contains(child) || !child.IsFocusable || child.IsHidden)
            return;

        if (_focusedChild != null)
            _focusedChild.IsFocused = false;

        _focusedChild = child;
        child.IsFocused = true;
    }

    private void EnsureFocusValid()
    {
        // If current focused child is gone or hidden, find next visible focusable
        if (_focusedChild != null && !_focusedChild.IsHidden && _focusedChild.IsFocusable)
            return;

        if (_focusedChild != null) _focusedChild.IsFocused = false;
        _focusedChild = null;

        foreach (var child in _children)
        {
            if (child.IsFocusable && !child.IsHidden)
            {
                _focusedChild = child;
                child.IsFocused = true;
                return;
            }
        }
    }

    private bool MoveFocus(int direction)
    {
        var visibleFocusable = new List<Component>();
        foreach (var child in _children)
        {
            if (child.IsFocusable && !child.IsHidden)
                visibleFocusable.Add(child);
        }

        if (visibleFocusable.Count <= 1)
            return false;

        var currentIndex = _focusedChild != null ? visibleFocusable.IndexOf(_focusedChild) : -1;
        if (currentIndex < 0)
            currentIndex = 0;

        if (_focusedChild != null)
            _focusedChild.IsFocused = false;

        var nextIndex = (currentIndex + direction + visibleFocusable.Count) % visibleFocusable.Count;
        _focusedChild = visibleFocusable[nextIndex];
        _focusedChild.IsFocused = true;
        return true;
    }

    private void EnsureFocusedVisible(int viewportHeight)
    {
        if (_focusedChild == null)
            return;

        var childIndex = _children.IndexOf(_focusedChild);
        if (childIndex < 0 || childIndex >= _componentRowRanges.Count)
            return;

        var (start, end) = _componentRowRanges[childIndex];
        if (start == end)
            return;

        var focusedRange = _focusedChild.GetFocusedRowRange();
        if (focusedRange.HasValue)
        {
            start += focusedRange.Value.Start;
            end = start + Math.Max(1, focusedRange.Value.End - focusedRange.Value.Start);
        }

        if (end > _scrollOffset + viewportHeight)
            _scrollOffset = end - viewportHeight;

        if (start < _scrollOffset)
            _scrollOffset = start;
    }

    private void ApplyViewportDiff(TerminalFrame? previousFrame, TerminalFrame nextFrame,
        int viewportHeight, bool forceFullRender)
    {
        var emptyPrev = Array.Empty<StyledCell>();

        for (var screenRow = 0; screenRow < viewportHeight; screenRow++)
        {
            var frameRow = _scrollOffset + screenRow;
            var prevFrameRow = _previousScrollOffset + screenRow;

            var prevCells = forceFullRender
                ? emptyPrev
                : previousFrame?.GetRowStyledCells(prevFrameRow) ?? emptyPrev;
            var nextCells = nextFrame.GetRowStyledCells(frameRow);

            WriteDirtyStyledSegments(screenRow, prevCells, nextCells, frameRow, nextFrame);
        }
    }

    private static readonly StyledCell BlankCell = new(' ', Style.Default);

    private void WriteDirtyStyledSegments(int screenRow, StyledCell[] prevCells, StyledCell[] nextCells,
        int frameRow, TerminalFrame nextFrame)
    {
        var maxLength = Math.Max(prevCells.Length, nextCells.Length);
        var segmentStart = -1;

        for (var column = 0; column < maxLength; column++)
        {
            var prev = column < prevCells.Length ? prevCells[column] : BlankCell;
            var next = column < nextCells.Length ? nextCells[column] : BlankCell;

            if (prev != next)
            {
                if (segmentStart == -1)
                    segmentStart = column;
            }
            else if (segmentStart != -1)
            {
                WriteStyledSegment(screenRow, segmentStart, column, frameRow, nextFrame);
                segmentStart = -1;
            }
        }

        if (segmentStart != -1)
            WriteStyledSegment(screenRow, segmentStart, maxLength, frameRow, nextFrame);
    }

    private void WriteStyledSegment(int screenRow, int start, int endExclusive,
        int frameRow, TerminalFrame nextFrame)
    {
        if (start >= endExclusive || screenRow >= _consoleDriver.WindowHeight)
            return;

        _consoleDriver.SetCursorPosition(start, screenRow);

        if (frameRow >= 0 && frameRow < nextFrame.Rows.Count)
        {
            var textLength = 0;
            foreach (var span in nextFrame.Rows[frameRow])
                textLength += span.Length;

            var spans = ExtractSpansForRange(nextFrame.Rows[frameRow], start, endExclusive, textLength);
            _consoleDriver.WriteStyled(spans);
        }
        else
        {
            _consoleDriver.Write(new string(' ', endExclusive - start));
        }
    }

    private static Span[] ExtractSpansForRange(List<Span> rowSpans, int start, int endExclusive, int textLength)
    {
        var result = new List<Span>();
        var col = 0;

        foreach (var span in rowSpans)
        {
            var spanEnd = col + span.Length;

            if (spanEnd <= start)
            {
                col = spanEnd;
                continue;
            }

            if (col >= endExclusive)
                break;

            var sliceStart = Math.Max(0, start - col);
            var sliceEnd = Math.Min(span.Length, endExclusive - col);

            if (sliceStart < sliceEnd)
            {
                var text = span.Text[sliceStart..sliceEnd];
                result.Add(new Span(text, span.Style));
            }

            col = spanEnd;
        }

        if (endExclusive > textLength)
        {
            var trailingSpaces = endExclusive - Math.Max(start, textLength);
            if (trailingSpaces > 0)
                result.Add(new Span(new string(' ', trailingSpaces)));
        }

        return result.ToArray();
    }

    private void ApplyViewportCursor(TerminalFrame frame, int viewportHeight)
    {
        if (frame.CursorLeft is null || frame.CursorTop is null)
        {
            _consoleDriver.CursorVisible = false;
            return;
        }

        var screenRow = frame.CursorTop.Value - _scrollOffset;

        if (screenRow < 0 || screenRow >= viewportHeight)
        {
            _consoleDriver.CursorVisible = false;
            return;
        }

        _consoleDriver.CursorVisible = true;
        _consoleDriver.SetCursorPosition(frame.CursorLeft.Value, screenRow);
    }
}
