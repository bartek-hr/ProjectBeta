using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Divider : Component
{
    public override int Render(ComponentRenderContext context)
    {
        context.Buffer.Repeat('\u2500', context.Width, Style.Muted).WriteLine();
        return 1;
    }
}
