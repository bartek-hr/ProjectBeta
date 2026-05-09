using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class LogoutButton : Component
{
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;

    public LogoutButton(AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public override bool IsFocusable => true;

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;
        buf.Write(IsFocused ? "> " : "  ", IsFocused ? Style.Primary : Style.Muted);
        buf.Write($"[ {l10n("components.logout.button")} ]", IsFocused ? Style.Highlight : Style.Default);
        buf.WriteLine();
        return 1;
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
        {
            Console.Clear();
            _appLoop.Display(_serviceProvider.GetRequiredService<LoginView>());
            return true;
        }
        return false;
    }
}
