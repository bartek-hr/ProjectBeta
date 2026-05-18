using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Button : Component
{
    private Action? _callback;
    private Action<Form>? _formCallback;

    public Button(string label)
    {
        Label = label;
    }

    public override bool IsFocusable => true;

    public string Label { get; }

    public Button OnClick(Action callback)
    {
        _callback = callback;
        _formCallback = null;
        return this;
    }

    public Button OnClick(Action<Form> callback)
    {
        _formCallback = callback;
        _callback = null;
        return this;
    }

    public override int Render(ComponentRenderContext context)
    {
        return Render(context, IsFocused);
    }

    internal int Render(ComponentRenderContext context, bool isFocused)
    {
        var buf = context.Buffer;
        var focusStyle = isFocused ? Style.Highlight : Style.Default;

        buf.Write(isFocused ? "> " : "  ", isFocused ? Style.Primary : Style.Muted);
        buf.Write($"[ {Label} ]", focusStyle);
        buf.WriteLine();
        return 1;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
        {
            Invoke();
            return true;
        }

        return false;
    }

    internal void Invoke()
    {
        if (_formCallback != null && ParentForm != null)
        {
            if (ParentForm.ValidateAll())
                _formCallback(ParentForm);
        }
        else
        {
            _callback?.Invoke();
        }
    }
}
