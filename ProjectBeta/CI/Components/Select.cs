using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Select : Component, IValidatable, IValueComponent
{
    private string[] _options = [];
    private int _selectedIndex;
    private bool _isRequired;
    private bool _hasSelection;
    private List<string> _errors = [];

    public Select(string label)
    {
        Label = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string? Value => _hasSelection && _selectedIndex < _options.Length ? _options[_selectedIndex] : null;
    [Obsolete("Use Value instead.")]
    public string? SelectedValue => Value;
    object? IValueComponent.Value => Value;

    public Select AddOption(string option)
    {
        _options = [.._options, option];
        return this;
    }

    public Select Required()
    {
        _isRequired = true;
        return this;
    }

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (_isRequired && !_hasSelection)
            errors.Add($"{Label} is required");
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

        // ┌── Label ─────────────────────┐
        InputBox.WriteTopBorder(buf, Label, boxWidth, borderStyle);
        lines++;

        if (IsFocused)
        {
            // Show all options inside the box
            for (var i = 0; i < _options.Length; i++)
            {
                var isSelected = i == _selectedIndex;
                var marker = isSelected ? "\u25cf " : "\u25cb ";
                var optionText = marker + _options[i];
                var optionStyle = isSelected ? Style.Primary : Style.Default;

                InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, optionText, optionStyle);
                lines++;
            }

            if (_options.Length == 0)
            {
                InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, "(no options)", Style.Muted);
                lines++;
            }
        }
        else
        {
            // Collapsed: show selected value
            var displayText = _hasSelection ? Value ?? "(none)" : "(none)";
            var textStyle = _hasSelection ? Style.Default : Style.Muted;
            InputBox.WriteFixedContentRow(buf, boxWidth, borderStyle, displayText, textStyle);
            lines++;
        }

        // └──────────────────────────────┘
        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);
        lines++;

        // Error line (always reserved)
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
                _selectedIndex = (_selectedIndex - 1 + _options.Length) % _options.Length;
                return true;
            case ConsoleKey.DownArrow:
                _selectedIndex = (_selectedIndex + 1) % _options.Length;
                return true;
            case ConsoleKey.Enter or ConsoleKey.Spacebar:
                _hasSelection = true;
                return true;
            default:
                return false;
        }
    }
}
