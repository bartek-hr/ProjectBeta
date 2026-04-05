namespace ProjectBeta.CI.Components;

public sealed class Spacer : Component
{
    private readonly int _lines;

    public Spacer(int lines = 1)
    {
        _lines = Math.Max(1, lines);
    }

    public override int Render(ComponentRenderContext context)
    {
        context.Buffer.BlankLine(_lines);
        return _lines;
    }
}
