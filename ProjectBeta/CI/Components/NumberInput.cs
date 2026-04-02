using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class NumberInput : Component, IValidatable, IValueComponent
{
    private double? _value;
    private string _textBuffer = string.Empty;
    private int _cursorPos;
    private double _min = double.MinValue;
    private double _max = double.MaxValue;
    private double _step = 1.0;
    private int _precision;
    private bool _isRequired;
    private List<string> _errors = [];

    public NumberInput(string label)
    {
        Label = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public double? Value => _value;
    object? IValueComponent.Value => _value;

    public NumberInput Min(double min)
    {
        _min = min;
        return this;
    }

    public NumberInput Max(double max)
    {
        _max = max;
        return this;
    }

    public NumberInput Step(double step)
    {
        _step = step;
        return this;
    }

    public NumberInput Precision(int precision)
    {
        _precision = precision;
        return this;
    }

    public NumberInput Required()
    {
        _isRequired = true;
        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (_isRequired && _value == null && string.IsNullOrEmpty(_textBuffer))
            errors.Add($"{Label} is required");

        if (_value != null)
        {
            if (_value < _min)
                errors.Add($"{Label} must be at least {_min}");
            if (_value > _max)
                errors.Add($"{Label} must be at most {_max}");
        }
        else if (!string.IsNullOrEmpty(_textBuffer))
        {
            errors.Add($"{Label} is not a valid number");
        }

        return errors;
    }

    public void SetErrors(List<string> errors)
    {
        _errors = errors;
    }

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;
        var boxWidth = InputBox.GetBoxWidth(context.Width);
        var innerWidth = InputBox.GetInnerWidth(boxWidth);
        var borderStyle = InputBox.GetBorderStyle(IsFocused, _errors.Count > 0);

        InputBox.WriteTopBorder(buf, Label, boxWidth, borderStyle);

        string fullText;
        Style textStyle;

        if (string.IsNullOrEmpty(_textBuffer))
        {
            fullText = "0";
            textStyle = Style.Muted;
        }
        else
        {
            fullText = _textBuffer;
            textStyle = _value != null ? Style.Default : Style.Error;
        }

        var viewOffset = 0;
        if (IsFocused && !string.IsNullOrEmpty(_textBuffer))
        {
            if (_cursorPos > innerWidth)
                viewOffset = _cursorPos - innerWidth;
        }

        var displayText = fullText.Length > viewOffset
            ? fullText[viewOffset..]
            : string.Empty;

        InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, displayText, textStyle);

        if (IsFocused)
        {
            context.SetCursor(4 + (_cursorPos - viewOffset), 1);
        }

        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);
        InputBox.WriteErrorLine(buf, _errors);

        return InputBox.StandardLineCount;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                var upVal = (_value ?? 0) + _step;
                upVal = Math.Min(upVal, _max);
                _value = upVal;
                SyncTextBuffer();
                return true;

            case ConsoleKey.DownArrow:
                var downVal = (_value ?? 0) - _step;
                downVal = Math.Max(downVal, _min);
                _value = downVal;
                SyncTextBuffer();
                return true;
        }

        if (key.KeyChar is >= '0' and <= '9' or '.' or '-')
        {
            _textBuffer = _textBuffer.Insert(_cursorPos, key.KeyChar.ToString());
            _cursorPos++;
            TryParse();
            return true;
        }

        switch (key.Key)
        {
            case ConsoleKey.Backspace when _cursorPos > 0:
                _textBuffer = _textBuffer.Remove(_cursorPos - 1, 1);
                _cursorPos--;
                TryParse();
                return true;
            case ConsoleKey.Delete when _cursorPos < _textBuffer.Length:
                _textBuffer = _textBuffer.Remove(_cursorPos, 1);
                TryParse();
                return true;
            case ConsoleKey.LeftArrow when _cursorPos > 0:
                _cursorPos--;
                return true;
            case ConsoleKey.RightArrow when _cursorPos < _textBuffer.Length:
                _cursorPos++;
                return true;
            case ConsoleKey.Home when _cursorPos != 0:
                _cursorPos = 0;
                return true;
            case ConsoleKey.End when _cursorPos != _textBuffer.Length:
                _cursorPos = _textBuffer.Length;
                return true;
            default:
                return false;
        }
    }

    private void TryParse()
    {
        if (double.TryParse(_textBuffer, out var parsed))
            _value = parsed;
        else
            _value = null;
    }

    private void SyncTextBuffer()
    {
        _textBuffer = _value?.ToString(_precision > 0 ? $"F{_precision}" : "G") ?? string.Empty;
        _cursorPos = _textBuffer.Length;
    }
}
