namespace ProjectBeta.CI.Components;

public abstract class Component
{
    private bool _hidden;
    private Func<bool>? _hiddenPredicate;

    public virtual bool IsFocusable => false;

    public bool IsFocused { get; internal set; }
    internal Form? ParentForm { get; private set; }

    /// <summary>
    /// Whether this component is currently hidden (not rendered, not focusable, not validated).
    /// </summary>
    public bool IsHidden => _hidden || (_hiddenPredicate?.Invoke() ?? false);

    /// <summary>
    /// Statically hide or show this component.
    /// </summary>
    public Component Hidden(bool hidden = true)
    {
        _hidden = hidden;
        return this;
    }

    /// <summary>
    /// Dynamically hide this component based on a predicate evaluated each frame.
    /// </summary>
    public Component Hidden(Func<bool> predicate)
    {
        _hiddenPredicate = predicate;
        return this;
    }

    public abstract int Render(ComponentRenderContext context);

    public virtual bool ProcessKey(ConsoleKeyInfo key)
    {
        return false;
    }

    internal void AttachToForm(Form form)
    {
        ParentForm = form;
        OnAttachedToForm(form);
    }

    protected virtual void OnAttachedToForm(Form form)
    {
    }

    public virtual (int Start, int End)? GetFocusedRowRange()
    {
        return null;
    }
}
