using System.Text.RegularExpressions;
using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public class InputText : Component, IValidatable, IValueComponent
{
    private string _value = string.Empty;
    private int _cursorIndex;
    private string _placeholder = string.Empty;
    private bool _isRequired;
    private bool _isMasked;
    private bool _allSelected;
    private bool _wasFocused;
    private int? _minLength;
    private int? _maxLength;
    private string? _pattern;
    private string? _patternMessage;
    private readonly List<Func<string, string?>> _customValidators = [];
    private List<string> _errors = [];

    public InputText(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }

    public string Value
    {
        get => _value;
        set
        {
            _value = value ?? string.Empty;
            ClampCursorIndex();
        }
    }

    object? IValueComponent.Value => _value;

    public InputText Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public InputText Placeholder(string placeholder)
    {
        _placeholder = placeholder;
        return this;
    }

    public InputText Required()
    {
        _isRequired = true;
        return this;
    }

    public InputText Masked()
    {
        _isMasked = true;
        return this;
    }

    public InputText Min(int min)
    {
        _minLength = min;
        return this;
    }

    public InputText Max(int max)
    {
        _maxLength = max;
        return this;
    }

    public InputText Pattern(string pattern, string? message = null)
    {
        _pattern = pattern;
        _patternMessage = message;
        return this;
    }

    public InputText Default(string value)
    {
        _value = value ?? string.Empty;
        ClampCursorIndex();
        return this;
    }

    /// <summary>
    /// Adds a custom validator. Return an error message string to indicate failure, or null for success.
    /// </summary>
    public InputText Validator(Func<string, string?> validator)
    {
        _customValidators.Add(validator);
        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (_isRequired && string.IsNullOrEmpty(_value))
            errors.Add(l10n("validation.common.required", new Dictionary<string, string> { ["field"] = Label }));

        if (_minLength.HasValue && _value.Length < _minLength.Value && _value.Length > 0)
            errors.Add(l10n("validation.common.min_length", new Dictionary<string, string>
            {
                ["field"] = Label,
                ["min"] = _minLength.Value.ToString()
            }));

        if (_pattern != null && _value.Length > 0 && !Regex.IsMatch(_value, _pattern))
            errors.Add(_patternMessage ?? l10n("validation.common.invalid", new Dictionary<string, string> { ["field"] = Label }));

        foreach (var validator in _customValidators)
        {
            var error = validator(_value);
            if (error != null)
                errors.Add(error);
        }

        return errors;
    }

    public void SetErrors(List<string> errors)
    {
        _errors = errors;
    }

    public override int Render(ComponentRenderContext context)
    {
        if (IsFocused && !_wasFocused && !string.IsNullOrEmpty(_value))
            _allSelected = true;
        if (!IsFocused)
            _allSelected = false;
        _wasFocused = IsFocused;

        var buf = context.Buffer;
        var boxWidth = InputBox.GetBoxWidth(context.Width);
        var innerWidth = InputBox.GetInnerWidth(boxWidth);
        var borderStyle = InputBox.GetBorderStyle(IsFocused, _errors.Count > 0);

        // ┌── Label ─────────────────────┐
        InputBox.WriteTopBorder(buf, Label, boxWidth, borderStyle);

        // │ value                        │
        string fullText;
        Style textStyle;

        if (string.IsNullOrEmpty(_value))
        {
            fullText = _placeholder;
            textStyle = Style.Muted;
        }
        else if (_isMasked)
        {
            fullText = new string('*', _value.Length);
            textStyle = _allSelected ? Style.Highlight : Style.Default;
        }
        else
        {
            fullText = _value;
            textStyle = _allSelected ? Style.Highlight : Style.Default;
        }

        // Viewport: when focused, keep cursor visible by scrolling; when unfocused, show from start
        var viewOffset = 0;
        if (IsFocused && !string.IsNullOrEmpty(_value))
        {
            // Ensure the cursor position is always within the visible viewport
            if (_cursorIndex > innerWidth)
                viewOffset = _cursorIndex - innerWidth;
        }

        var displayText = fullText.Length > viewOffset
            ? fullText[viewOffset..]
            : string.Empty;

        InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, displayText, textStyle);

        if (IsFocused)
        {
            // Cursor column relative to viewport: "  │ " = 4 chars, then cursor offset within visible area
            context.SetCursor(4 + (_cursorIndex - viewOffset), 1);
        }

        // └──────────────────────────────┘
        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);

        // error line (always reserved)
        InputBox.WriteErrorLine(buf, _errors);

        return InputBox.StandardLineCount;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        ClampCursorIndex();

        if (!char.IsControl(key.KeyChar))
        {
            if (_allSelected)
            {
                Value = key.KeyChar.ToString();
                _cursorIndex = 1;
                _allSelected = false;
                return true;
            }

            if (_maxLength.HasValue && _value.Length >= _maxLength.Value)
                return false;

            Value = Value.Insert(_cursorIndex, key.KeyChar.ToString());
            _cursorIndex++;
            return true;
        }

        switch (key.Key)
        {
            case ConsoleKey.Backspace when _allSelected:
                Value = string.Empty;
                _cursorIndex = 0;
                _allSelected = false;
                return true;
            case ConsoleKey.Backspace when _cursorIndex > 0:
                var removalIndex = _cursorIndex - 1;
                Value = Value.Remove(removalIndex, 1);
                _cursorIndex = removalIndex;
                return true;
            case ConsoleKey.Delete when _allSelected:
                Value = string.Empty;
                _cursorIndex = 0;
                _allSelected = false;
                return true;
            case ConsoleKey.Delete when _cursorIndex < Value.Length:
                Value = Value.Remove(_cursorIndex, 1);
                return true;
            case ConsoleKey.LeftArrow when _allSelected:
                _allSelected = false;
                _cursorIndex = 0;
                return true;
            case ConsoleKey.RightArrow when _allSelected:
                _allSelected = false;
                _cursorIndex = Value.Length;
                return true;
            case ConsoleKey.LeftArrow when _cursorIndex > 0:
                _cursorIndex--;
                return true;
            case ConsoleKey.RightArrow when _cursorIndex < Value.Length:
                _cursorIndex++;
                return true;
            case ConsoleKey.Home when _cursorIndex != 0:
                _allSelected = false;
                _cursorIndex = 0;
                return true;
            case ConsoleKey.End when _cursorIndex != Value.Length:
                _allSelected = false;
                _cursorIndex = Value.Length;
                return true;
            default:
                return false;
        }
    }

    private void ClampCursorIndex()
    {
        _cursorIndex = Math.Clamp(_cursorIndex, 0, _value.Length);
    }
}
