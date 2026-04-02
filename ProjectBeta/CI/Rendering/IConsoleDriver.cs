namespace ProjectBeta.CI.Rendering;

public interface IConsoleDriver
{
    int WindowWidth { get; }
    int WindowHeight { get; }
    bool CursorVisible { set; }
    void SetCursorPosition(int left, int top);
    void Write(string text);
    void WriteStyled(ReadOnlySpan<Span> spans);
    void Clear();
}
