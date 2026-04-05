using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class Message : Component
{
    private readonly Func<string?> _messageProvider;
    private readonly Style _style;

    public Message(Func<string?> messageProvider, Style? style = null)
    {
        _messageProvider = messageProvider;
        _style = style ?? Style.Warning;
    }

    public override int Render(ComponentRenderContext context)
    {
        var message = _messageProvider();
        if (string.IsNullOrWhiteSpace(message))
        {
            return 0;
        }

        var buf = context.Buffer;
        var lines = message.Replace("\r", string.Empty).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            buf.Write("  ").WriteLine(lines[index], _style);
        }

        return lines.Length;
    }
}
