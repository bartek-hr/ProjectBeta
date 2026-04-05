using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Heading : Component
{
    private readonly string _text;

    public Heading(string text)
    {
        _text = text;
    }

    public override int Render(ComponentRenderContext context)
    {
        context.Buffer.WriteLine(_text, Style.Primary);
        return 1;
    }
}
