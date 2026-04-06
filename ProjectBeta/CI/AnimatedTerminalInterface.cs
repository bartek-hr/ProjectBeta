namespace ProjectBeta.CI;

public abstract class AnimatedTerminalInterface : TerminalInterface
{
    
    public int Fps = 10;
    
    public abstract void ProcessKey(ConsoleKeyInfo key);
    
}