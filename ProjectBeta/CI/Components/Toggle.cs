using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Toggle : Component, IValueComponent
{
    private bool _value;

    public Toggle(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }
    public bool Value => _value;
    object? IValueComponent.Value => _value;

    public Toggle Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public Toggle Default(bool value)
    {
        _value = value;
        return this;
    }

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;
        var boxWidth = InputBox.GetBoxWidth(context.Width);
        var borderStyle = InputBox.GetBorderStyle(IsFocused, false);

        InputBox.WriteTopBorder(buf, Label, boxWidth, borderStyle);

        // Build the toggle display in the content row
        // We need styled segments, so use WriteContentRow approach manually
        buf.Write("  ");
        buf.Write("\u2502 ", borderStyle);

        var innerWidth = InputBox.GetInnerWidth(boxWidth);

        if (_value)
        {
            buf.Write("(\u25cf) ", Style.Success);
            buf.Write(l10n("components.toggle.on"), Style.Success.WithBold());
        }
        else
        {
            buf.Write("(\u25cb) ", Style.Muted);
            buf.Write(l10n("components.toggle.off"), Style.Muted);
        }

        // Pad remaining space
        var textLen = _value ? 6 : 7; // "(● ) ON" = 6, "(○ ) OFF" = 7
        var padding = innerWidth - textLen;
        if (padding > 0)
            buf.Repeat(' ', padding);

        buf.Write(" \u2502", borderStyle);
        buf.WriteLine();

        InputBox.WriteBottomBorder(buf, boxWidth, borderStyle);
        InputBox.WriteErrorLine(buf, []);

        return InputBox.StandardLineCount;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (key.Key is ConsoleKey.Spacebar or ConsoleKey.Enter)
        {
            _value = !_value;
            return true;
        }

        return false;
    }
}
