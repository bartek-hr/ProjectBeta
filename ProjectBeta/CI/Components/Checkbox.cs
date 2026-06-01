using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Checkbox : Component, IValueComponent
{
    private bool _checked;

    public Checkbox(string label)
    {
        Label = label;
        FieldKey = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }
    public string FieldKey { get; private set; }
    public bool Value => _checked;
    [Obsolete("Use Value instead.")]
    public bool Checked => _checked;
    object? IValueComponent.Value => Value;

    public Checkbox Key(string fieldKey)
    {
        FieldKey = string.IsNullOrWhiteSpace(fieldKey) ? Label : fieldKey;
        return this;
    }

    public Checkbox Default(bool value)
    {
        _checked = value;
        return this;
    }

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;
        var focusStyle = IsFocused ? Style.Primary : Style.Muted;

        buf.Write(IsFocused ? "> " : "  ", focusStyle);
        buf.Write(_checked ? "[x] " : "[ ] ", IsFocused ? Style.Default.WithBold() : Style.Default);
        buf.WriteLine(Label);
        return 1;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Spacebar)
        {
            _checked = !_checked;
            return true;
        }

        return false;
    }
}
