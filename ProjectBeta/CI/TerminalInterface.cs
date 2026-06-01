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
        Console.Write(text);
    }

    public void WriteLine(string text)
    {
        Console.WriteLine(text);
    }
    
    public void Clear()
    {
        Console.Clear();
    }
}
