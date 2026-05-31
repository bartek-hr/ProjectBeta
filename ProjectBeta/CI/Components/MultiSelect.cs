using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class MultiSelect : Component, IValidatable, IValueComponent
{
    private string[] _options = [];
    private readonly HashSet<int> _selectedIndices = [];
    private int _highlightedIndex;
    private bool _isRequired;
    private List<string> _errors = [];

    public MultiSelect(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }

    public string[] Value
    {
        get
        {
            var result = new List<string>();
            for (var i = 0; i < _options.Length; i++)
            {
                if (_selectedIndices.Contains(i))
                    result.Add(_options[i]);
            }

            return result.ToArray();
        }
    }

    [Obsolete("Use Value instead.")]
    public string[] SelectedValues => Value;

    object? IValueComponent.Value => Value;

    public MultiSelect Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public MultiSelect AddOption(string option)
    {
        _options = [.._options, option];
        return this;
    }

    public MultiSelect Required()
    {
        _isRequired = true;
        return this;
    }

    public MultiSelect Defaults(params string[] defaults)
    {
        var set = new HashSet<string>(defaults);
        for (var i = 0; i < _options.Length; i++)
        {
            if (set.Contains(_options[i]))
                _selectedIndices.Add(i);
        }

        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (_isRequired && _selectedIndices.Count == 0)
            errors.Add(l10n("validation.common.selection_required", new Dictionary<string, string> { ["field"] = Label }));
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

        if (IsFocused)
        {
            for (var i = 0; i < _options.Length; i++)
            {
                var isChecked = _selectedIndices.Contains(i);
                var isHighlighted = i == _highlightedIndex;
                var marker = isChecked ? "[x] " : "[ ] ";
                var optionText = marker + _options[i];
                var optionStyle = isHighlighted ? Style.Primary : Style.Default;

                InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, optionText, optionStyle);
                lines++;
            }

            if (_options.Length == 0)
            {
                InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, l10n("components.multiselect.no_options"), Style.Muted);
                lines++;
            }
        }
        else
        {
            var selected = Value;
            var displayText = selected.Length > 0 ? string.Join(", ", selected) : l10n("components.form.none");
            var textStyle = selected.Length > 0 ? Style.Default : Style.Muted;
            InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, displayText, textStyle);
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
            case ConsoleKey.Spacebar:
                if (_selectedIndices.Contains(_highlightedIndex))
                    _selectedIndices.Remove(_highlightedIndex);
                else
                    _selectedIndices.Add(_highlightedIndex);
                return true;
            default:
                return false;
        }
    }
}
