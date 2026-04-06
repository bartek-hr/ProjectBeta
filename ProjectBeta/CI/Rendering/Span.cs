namespace ProjectBeta.CI.Rendering;

public readonly record struct Span(string Text, Style Style)
{
    public int Length => Text.Length;

    public Span(string text) : this(text, Style.Default)
    {
    }
}
