namespace ProjectBeta.CI;

public abstract class GameLoop
{
    protected abstract TerminalInterface? GetActiveInterface();

    public void Run()
    {
        TerminalInterface? lastProcessedInterface = null;

        while (GetActiveInterface() != null)
        {
            var activeInterface = GetActiveInterface()!;
            if (!ReferenceEquals(lastProcessedInterface, activeInterface))
            {
                activeInterface.Render();
                lastProcessedInterface = activeInterface;
            }

            if (activeInterface is AnimatedTerminalInterface animatedInterface)
            {
                if (animatedInterface.Fps <= 0)
                    throw new Exception("Fps must be greater than 0");

                RunAnimatedLoop(animatedInterface);
            }
            else if (activeInterface is StaticTerminalInterface staticInterface)
            {
                if (staticInterface.ProcessKey(Console.ReadKey(intercept: true)))
                {
                    staticInterface.Render();
                }
            }
        }
    }

    private void RunAnimatedLoop(AnimatedTerminalInterface activeInterface)
    {
        using var renderLoopCancellation = new CancellationTokenSource();
        var renderLock = new object();
        var frameInterval = TimeSpan.FromMilliseconds(Math.Max(1, (int)Math.Round(1000d / activeInterface.Fps)));

        var renderThread = new Thread(() =>
        {
            while (!renderLoopCancellation.IsCancellationRequested && GetActiveInterface() == activeInterface)
            {
                var frameStartedAt = DateTime.UtcNow;
                lock (renderLock)
                {
                    activeInterface.Render();
                }

                var elapsed = DateTime.UtcNow - frameStartedAt;
                var sleepTime = frameInterval - elapsed;
                if (sleepTime > TimeSpan.Zero)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        })
        {
            IsBackground = true
        };

        renderThread.Start();
        while (GetActiveInterface() == activeInterface)
        {
            var key = Console.ReadKey(intercept: true);
            lock (renderLock)
            {
                activeInterface.ProcessKey(key);
            }
        }

        renderLoopCancellation.Cancel();
        renderThread.Join();
    }
}
