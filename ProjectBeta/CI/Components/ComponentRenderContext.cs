using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class ComponentRenderContext
{
    public ComponentRenderContext(TerminalBuffer buffer, int top, int width)
    {
        Buffer = buffer;
        Top = top;
        Width = width;
    }

    public TerminalBuffer Buffer { get; }
    public int Top { get; }
    public int Width { get; }

    public void SetCursor(int left, int relativeTop = 0)
    {
        Buffer.SetCursor(left, Top + relativeTop);
    }
}
