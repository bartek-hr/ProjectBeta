using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class RadioGroup : Component, IValidatable, IValueComponent
{
    private string[] _options = [];
    private int _highlightedIndex;
    private int _selectedIndex = -1;
    private bool _isRequired;
    private List<string> _errors = [];

    public RadioGroup(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }
    public string? Value => _selectedIndex >= 0 && _selectedIndex < _options.Length ? _options[_selectedIndex] : null;
    [Obsolete("Use Value instead.")]
    public string? SelectedValue => Value;
    object? IValueComponent.Value => Value;

    public RadioGroup Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public RadioGroup AddOption(string option)
    {
        _options = [.._options, option];
        return this;
    }

    public RadioGroup Required()
    {
        _isRequired = true;
        return this;
    }

    public RadioGroup Default(string option)
    {
        var index = Array.FindIndex(_options, current => string.Equals(current, option, StringComparison.Ordinal));
        if (index >= 0)
        {
            _selectedIndex = index;
            _highlightedIndex = index;
        }

        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (_isRequired && _selectedIndex < 0)
            errors.Add(l10n("validation.common.required", new Dictionary<string, string> { ["field"] = Label }));
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
        var borderStyle = InputBox.GetBorderStyle(IsFocused, _errors.Count > 0);
        var lines = 0;

        InputBox.WriteTopBorder(buf, Label, boxWidth, borderStyle);
        lines++;

        for (var i = 0; i < _options.Length; i++)
        {
            var isSelected = i == _selectedIndex;
            var isHighlighted = IsFocused && i == _highlightedIndex;
            var marker = isSelected ? "(\u25cf) " : "(\u25cb) ";
            var optionText = marker + _options[i];

            Style optionStyle;
            if (isHighlighted)
                optionStyle = Style.Primary;
            else if (isSelected)
                optionStyle = Style.Success;
            else
                optionStyle = Style.Default;

            InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, optionText, optionStyle);
            lines++;
        }

        if (_options.Length == 0)
        {
            InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, l10n("components.radiogroup.no_options"), Style.Muted);
            lines++;
        }

        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);
        lines++;

        InputBox.WriteErrorLine(buf, _errors);
        lines++;

        return lines;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (_options.Length == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _highlightedIndex = (_highlightedIndex - 1 + _options.Length) % _options.Length;
                return true;
            case ConsoleKey.DownArrow:
                _highlightedIndex = (_highlightedIndex + 1) % _options.Length;
                return true;
            case ConsoleKey.Spacebar or ConsoleKey.Enter:
                _selectedIndex = _highlightedIndex;
                return true;
            default:
                return false;
        }
    }
}
