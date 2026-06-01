using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class DateInput : Component, IValidatable, IValueComponent
{
    private int _year;
    private int _month;
    private int _day;
    private int _activeSegment; // 0=year, 1=month, 2=day
    private bool _isRequired;
    private bool _hasValue;
    private DateOnly? _min;
    private DateOnly? _max;
    private string _digitBuffer = string.Empty;
    private List<string> _errors = [];

    public DateInput(string label)
    {
        Label = label;
        FieldKey = label;
        var today = DateOnly.FromDateTime(DateTime.Today);
        _year = today.Year;
        _month = today.Month;
        _day = today.Day;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }

    public DateOnly? Value
    {
        get
        {
            if (!_hasValue)
                return null;

            try
            {
                return new DateOnly(_year, _month, _day);
            }
            catch
            {
                return null;
            }
        }
    }

    object? IValueComponent.Value => Value;

    public DateInput Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public DateInput Required()
    {
        _isRequired = true;
        return this;
    }

    public DateInput Min(DateOnly min)
    {
        _min = min;
        return this;
    }

    public DateInput Max(DateOnly max)
    {
        _max = max;
        return this;
    }

    public DateInput Default(DateOnly date)
    {
        _year = date.Year;
        _month = date.Month;
        _day = date.Day;
        _hasValue = true;
        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (_isRequired && !_hasValue)
        {
            errors.Add(l10n("validation.common.required", new Dictionary<string, string> { ["field"] = Label }));
            return errors;
        }

        if (!_hasValue)
            return errors;

        var date = Value;
        if (date == null)
        {
            errors.Add(l10n("validation.common.invalid_date", new Dictionary<string, string> { ["field"] = Label }));
            return errors;
        }

        if (_min.HasValue && date < _min.Value)
            errors.Add(l10n("validation.common.date_on_or_after", new Dictionary<string, string>
            {
                ["field"] = Label,
                ["date"] = _min.Value.ToString("yyyy-MM-dd")
            }));

        if (_max.HasValue && date > _max.Value)
            errors.Add(l10n("validation.common.date_on_or_before", new Dictionary<string, string>
            {
                ["field"] = Label,
                ["date"] = _max.Value.ToString("yyyy-MM-dd")
            }));

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

        // Content row: render date segments with styling
        buf.Write("  ");
        buf.Write("\u2502 ", borderStyle);

        if (!_hasValue && !IsFocused)
        {
            var placeholder = l10n("components.form.date_placeholder");
            buf.Write(placeholder, Style.Muted);
            var pad = innerWidth - placeholder.Length;
            if (pad > 0)
                buf.Repeat(' ', pad);
        }
        else
        {
            var yearStr = _year.ToString("D4");
            var monthStr = _month.ToString("D2");
            var dayStr = _day.ToString("D2");

            WriteSegment(buf, yearStr, 0);
            buf.Write("-", Style.Default);
            WriteSegment(buf, monthStr, 1);
            buf.Write("-", Style.Default);
            WriteSegment(buf, dayStr, 2);

            // "YYYY-MM-DD" = 10 chars
            var pad = innerWidth - 10;
            if (pad > 0)
                buf.Repeat(' ', pad);
        }

        buf.Write(" \u2502", borderStyle);
        buf.WriteLine();

        if (IsFocused)
        {
            // Position cursor at the start of the active segment
            // "  │ " = 4 chars, then: year at 0, month at 5, day at 8
            var cursorCol = _activeSegment switch
            {
                0 => 4,
                1 => 4 + 5,
                2 => 4 + 8,
                _ => 4,
            };
            context.SetCursor(cursorCol, 1);
        }

        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);
        InputBox.WriteErrorLine(buf, _errors);

        return InputBox.StandardLineCount;
    }

    private void WriteSegment(TerminalBuffer buf, string text, int segment)
    {
        var style = IsFocused && _activeSegment == segment
            ? Style.Primary.WithUnderline()
            : Style.Default;
        buf.Write(text, style);
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow when _activeSegment > 0:
                _activeSegment--;
                _digitBuffer = string.Empty;
                return true;
            case ConsoleKey.RightArrow when _activeSegment < 2:
                _activeSegment++;
                _digitBuffer = string.Empty;
                return true;
            case ConsoleKey.UpArrow:
                IncrementSegment(1);
                _hasValue = true;
                return true;
            case ConsoleKey.DownArrow:
                IncrementSegment(-1);
                _hasValue = true;
                return true;
            case ConsoleKey.Enter or ConsoleKey.Spacebar:
                _hasValue = true;
                return true;
        }

        if (key.KeyChar is >= '0' and <= '9')
        {
            _digitBuffer += key.KeyChar;
            _hasValue = true;
            ApplyDigitBuffer();
            return true;
        }

        return false;
    }

    private void IncrementSegment(int delta)
    {
        _digitBuffer = string.Empty;

        switch (_activeSegment)
        {
            case 0:
                _year = Math.Clamp(_year + delta, 1, 9999);
                break;
            case 1:
                _month += delta;
                if (_month > 12) { _month = 1; _year = Math.Min(_year + 1, 9999); }
                else if (_month < 1) { _month = 12; _year = Math.Max(_year - 1, 1); }
                break;
            case 2:
                _day += delta;
                var daysInMonth = DateTime.DaysInMonth(_year, _month);
                if (_day > daysInMonth) { _day = 1; IncrementSegment(0); _activeSegment = 2; _month += 1; if (_month > 12) { _month = 1; _year = Math.Min(_year + 1, 9999); } }
                else if (_day < 1) { _month -= 1; if (_month < 1) { _month = 12; _year = Math.Max(_year - 1, 1); } _day = DateTime.DaysInMonth(_year, _month); }
                break;
        }

        ClampDay();
    }

    private void ApplyDigitBuffer()
    {
        if (!int.TryParse(_digitBuffer, out var num))
            return;

        switch (_activeSegment)
        {
            case 0: // year: accept up to 4 digits
                _year = Math.Clamp(num, 0, 9999);
                if (_digitBuffer.Length >= 4)
                {
                    _digitBuffer = string.Empty;
                    if (_activeSegment < 2)
                        _activeSegment++;
                }

                break;
            case 1: // month: accept up to 2 digits
                _month = Math.Clamp(num, 1, 12);
                if (_digitBuffer.Length >= 2 || num > 1)
                {
                    _digitBuffer = string.Empty;
                    if (_activeSegment < 2)
                        _activeSegment++;
                }

                break;
            case 2: // day: accept up to 2 digits
                var maxDay = DateTime.DaysInMonth(_year, Math.Clamp(_month, 1, 12));
                _day = Math.Clamp(num, 1, maxDay);
                if (_digitBuffer.Length >= 2 || num > 3)
                {
                    _digitBuffer = string.Empty;
                }

                break;
        }

        ClampDay();
    }

    private void ClampDay()
    {
        _month = Math.Clamp(_month, 1, 12);
        var maxDay = DateTime.DaysInMonth(_year, _month);
        _day = Math.Clamp(_day, 1, maxDay);
    }
}
