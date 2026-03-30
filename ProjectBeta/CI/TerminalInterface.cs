using ProjectBeta.Utils;

namespace ProjectBeta.CI;

public abstract class TerminalInterface
{
    public abstract void Render();

    public void Dispose()
    {
    }

    public void Init()
    {
    }

    public void Write(string text)
    {
        Console.Write(StringColor.Colorize(text));
    }

    public void WriteLine(string text)
    {
        Console.WriteLine(StringColor.Colorize(text));
    }
    
    public void Clear()
    {
        Console.Clear();
    }
}
