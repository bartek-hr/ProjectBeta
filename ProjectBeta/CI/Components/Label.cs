using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Label : Component
{
    private readonly string[] _lines;
    private readonly Style _style;

    public Label(string text, Style? style = null)
    {
        _lines = text.Replace("\r", string.Empty).Split('\n');
        _style = style ?? Style.Muted;
    }

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;
        for (var index = 0; index < _lines.Length; index++)
        {
            buf.WriteLine(_lines[index], _style);
        }

        return _lines.Length;
    }
}
