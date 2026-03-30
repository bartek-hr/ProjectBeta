namespace ProjectBeta.CI;

public abstract class StaticTerminalInterface : TerminalInterface
{
    
    public abstract bool ProcessKey(ConsoleKeyInfo key);
    
}