using ProjectBeta.CI.Components;

namespace ProjectBeta.CI;

public class AppLoop
{
    private readonly List<TerminalInterface> _interfaceStack = [];

    public TerminalInterface? ActiveInterface => _interfaceStack.Count > 0 ? _interfaceStack[^1] : null;

    public void Display(TerminalInterface terminalInterface)
    {
        if (terminalInterface == null)
            throw new ArgumentNullException(nameof(terminalInterface));

        RemoveInterface(terminalInterface);

        if (terminalInterface is RootComponent rootComponent)
        {
            rootComponent.Closed -= HandleRootComponentClosed;
            rootComponent.Closed += HandleRootComponentClosed;
            rootComponent.PrepareForDisplay();
        }

        _interfaceStack.Add(terminalInterface);
    }

    public void Run()
    {
        TerminalInterface? lastRenderedInterface = null;
        var lastWidth = Console.WindowWidth;
        var lastHeight = Console.WindowHeight;

        while (ActiveInterface != null)
        {
            var activeInterface = ActiveInterface;
            if (activeInterface == null)
                break;

            if (!ReferenceEquals(lastRenderedInterface, activeInterface))
            {
                activeInterface.Render();
                lastRenderedInterface = activeInterface;
                lastWidth = Console.WindowWidth;
                lastHeight = Console.WindowHeight;
            }

            if (activeInterface is AnimatedTerminalInterface animatedInterface)
            {
                if (animatedInterface.Fps <= 0)
                    throw new Exception("Fps must be greater than 0");

                RunAnimatedLoop(animatedInterface);
                continue;
            }

            if (activeInterface is not StaticTerminalInterface staticInterface)
                break;

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (!ReferenceEquals(activeInterface, ActiveInterface))
                    continue;

                var handled = staticInterface.ProcessKey(key);
                var currentInterface = ActiveInterface;

                if (currentInterface == null)
                    break;

                if (!ReferenceEquals(currentInterface, activeInterface))
                {
                    lastRenderedInterface = null;
                    continue;
                }

                if (handled)
                    staticInterface.Render();
            }
            else
            {
                var currentWidth = Console.WindowWidth;
                var currentHeight = Console.WindowHeight;

                if (currentWidth != lastWidth || currentHeight != lastHeight)
                {
                    lastWidth = currentWidth;
                    lastHeight = currentHeight;

                    if (ActiveInterface is StaticTerminalInterface resizedInterface)
                    {
                        resizedInterface.Invalidate();
                        resizedInterface.Render();
                    }
                }

                Thread.Sleep(50);
            }
        }

        Console.Clear();
        Console.WriteLine(l10n("app.exited"));
    }

    private void RunAnimatedLoop(AnimatedTerminalInterface activeInterface)
    {
        using var renderLoopCancellation = new CancellationTokenSource();
        var renderLock = new object();
        var frameInterval = TimeSpan.FromMilliseconds(Math.Max(1, (int)Math.Round(1000d / activeInterface.Fps)));

        var renderThread = new Thread(() =>
        {
            while (!renderLoopCancellation.IsCancellationRequested && ReferenceEquals(ActiveInterface, activeInterface))
            {
                var frameStartedAt = DateTime.UtcNow;
                lock (renderLock)
                {
                    activeInterface.Render();
                }

                var elapsed = DateTime.UtcNow - frameStartedAt;
                var sleepTime = frameInterval - elapsed;
                if (sleepTime > TimeSpan.Zero)
                    Thread.Sleep(sleepTime);
            }
        })
        {
            IsBackground = true
        };

        renderThread.Start();

        while (ReferenceEquals(ActiveInterface, activeInterface))
        {
            var key = Console.ReadKey(intercept: true);
            lock (renderLock)
            {
                if (!ReferenceEquals(ActiveInterface, activeInterface))
                    break;

                activeInterface.ProcessKey(key);
            }
        }

        renderLoopCancellation.Cancel();
        renderThread.Join();
    }

    private void HandleRootComponentClosed(RootComponent rootComponent)
    {
        rootComponent.Closed -= HandleRootComponentClosed;
        RemoveInterface(rootComponent);

        if (ActiveInterface is StaticTerminalInterface staticInterface)
            staticInterface.Invalidate();
    }

    private void RemoveInterface(TerminalInterface terminalInterface)
    {
        for (var i = _interfaceStack.Count - 1; i >= 0; i--)
        {
            if (!ReferenceEquals(_interfaceStack[i], terminalInterface))
                continue;

            _interfaceStack.RemoveAt(i);
            break;
        }
    }
}
