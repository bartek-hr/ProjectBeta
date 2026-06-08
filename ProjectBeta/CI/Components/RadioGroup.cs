using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public class RadioGroup<TValue> : Component, IValidatable, IValueComponent
    where TValue : notnull
{
    private readonly List<OptionItem> _options = [];
    private int _highlightedIndex;
    private int _selectedIndex = -1;
    private bool _isRequired;
    private List<string> _errors = [];
    private sealed record OptionItem(TValue Value, string Label);

    public RadioGroup(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }
    public bool HasValue => _selectedIndex >= 0 && _selectedIndex < _options.Count;
    public TValue? Value => _selectedIndex >= 0 && _selectedIndex < _options.Count ? _options[_selectedIndex].Value : default;
    [Obsolete("Use Value instead.")]
    public TValue? SelectedValue => Value;
    object? IValueComponent.Value => Value;

    public RadioGroup<TValue> Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public RadioGroup<TValue> AddOption(TValue option)
    {
        return AddOption(option, option.ToString() ?? string.Empty);
    }

    public RadioGroup<TValue> AddOption(TValue value, string label)
    {
        _options.Add(new OptionItem(value, label));
        return this;
    }

    public RadioGroup<TValue> Required()
    {
        _isRequired = true;
        return this;
    }

    public RadioGroup<TValue> Default(TValue option)
    {
        TrySelectByValue(option);

        return this;
    }

    public RadioGroup<TValue> DefaultByLabel(string label)
    {
        TrySelectByLabel(label);

        return this;
    }

    protected bool TrySelectByValue(TValue option)
    {
        var index = _options.FindIndex(current => EqualityComparer<TValue>.Default.Equals(current.Value, option));
        if (index < 0)
            return false;

        _selectedIndex = index;
        _highlightedIndex = index;
        return true;
    }

    protected bool TrySelectByLabel(string label)
    {
        var index = _options.FindIndex(current => string.Equals(current.Label, label, StringComparison.Ordinal));
        if (index < 0)
            return false;

        _selectedIndex = index;
        _highlightedIndex = index;
        return true;
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

        for (var i = 0; i < _options.Count; i++)
        {
            var isSelected = i == _selectedIndex;
            var isHighlighted = IsFocused && i == _highlightedIndex;
            var marker = isSelected ? "(\u25cf) " : "(\u25cb) ";
            var optionText = marker + _options[i].Label;

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

        if (_options.Count == 0)
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
        if (_options.Count == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _highlightedIndex = (_highlightedIndex - 1 + _options.Count) % _options.Count;
                return true;
            case ConsoleKey.DownArrow:
                _highlightedIndex = (_highlightedIndex + 1) % _options.Count;
                return true;
            case ConsoleKey.Spacebar or ConsoleKey.Enter:
                _selectedIndex = _highlightedIndex;
                return true;
            default:
                return false;
        }
    }
}

public sealed class RadioGroup : RadioGroup<string>
{
    public RadioGroup(string label) : base(label)
    {
    }

    public new RadioGroup Default(string option)
    {
        if (!TrySelectByValue(option))
            TrySelectByLabel(option);

        return this;
    }
}
