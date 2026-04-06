using System.Text;

namespace ProjectBeta.CI.Rendering;

public sealed class SystemConsoleDriver : IConsoleDriver
{
    private static readonly bool SupportsAnsi = DetectAnsiSupport();

    public int WindowWidth => Console.WindowWidth;
    public int WindowHeight => Console.WindowHeight;

    public bool CursorVisible
    {
        set => Console.CursorVisible = value;
    }

    public void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }

    public void Write(string text)
    {
        Console.Write(text);
    }

    public void Clear()
    {
        Console.Clear();
    }

    public void WriteStyled(ReadOnlySpan<Span> spans)
    {
        if (!SupportsAnsi)
        {
            WriteFallback(spans);
            return;
        }

        var sb = new StringBuilder();
        foreach (var span in spans)
        {
            if (span.Text.Length == 0)
                continue;

            var style = span.Style;
            var hasStyle = style.Fg != null || style.Bg != null ||
                           style.Bold || style.Dim || style.Underline ||
                           style.Italic || style.Strikethrough;

            if (hasStyle)
            {
                sb.Append("\x1b[");
                var needSemi = false;

                if (style.Bold) { sb.Append('1'); needSemi = true; }
                if (style.Dim) { if (needSemi) sb.Append(';'); sb.Append('2'); needSemi = true; }
                if (style.Italic) { if (needSemi) sb.Append(';'); sb.Append('3'); needSemi = true; }
                if (style.Underline) { if (needSemi) sb.Append(';'); sb.Append('4'); needSemi = true; }
                if (style.Strikethrough) { if (needSemi) sb.Append(';'); sb.Append('9'); needSemi = true; }

                if (style.Fg != null)
                {
                    if (needSemi) sb.Append(';');
                    sb.Append(FgCode(style.Fg.Value));
                    needSemi = true;
                }

                if (style.Bg != null)
                {
                    if (needSemi) sb.Append(';');
                    sb.Append(BgCode(style.Bg.Value));
                }

                sb.Append('m');
            }

            sb.Append(span.Text);

            if (hasStyle)
                sb.Append("\x1b[0m");
        }

        Console.Write(sb.ToString());
    }

    private static void WriteFallback(ReadOnlySpan<Span> spans)
    {
        foreach (var span in spans)
        {
            if (span.Text.Length == 0)
                continue;

            var style = span.Style;
            var needReset = false;

            if (style.Fg != null)
            {
                Console.ForegroundColor = style.Fg.Value;
                needReset = true;
            }

            if (style.Bg != null)
            {
                Console.BackgroundColor = style.Bg.Value;
                needReset = true;
            }

            Console.Write(span.Text);

            if (needReset)
                Console.ResetColor();
        }
    }

    private static int FgCode(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => 30,
        ConsoleColor.DarkRed => 31,
        ConsoleColor.DarkGreen => 32,
        ConsoleColor.DarkYellow => 33,
        ConsoleColor.DarkBlue => 34,
        ConsoleColor.DarkMagenta => 35,
        ConsoleColor.DarkCyan => 36,
        ConsoleColor.Gray => 37,
        ConsoleColor.DarkGray => 90,
        ConsoleColor.Red => 91,
        ConsoleColor.Green => 92,
        ConsoleColor.Yellow => 93,
        ConsoleColor.Blue => 94,
        ConsoleColor.Magenta => 95,
        ConsoleColor.Cyan => 96,
        ConsoleColor.White => 97,
        _ => 39,
    };

    private static int BgCode(ConsoleColor color) => FgCode(color) + 10;

    private static bool DetectAnsiSupport()
    {
        if (Environment.GetEnvironmentVariable("WT_SESSION") != null)
            return true;
        if (Environment.GetEnvironmentVariable("COLORTERM") is "truecolor" or "24bit")
            return true;

        var term = Environment.GetEnvironmentVariable("TERM") ?? string.Empty;
        if (term.Contains("xterm") || term.Contains("256color") || term.Contains("screen") || term.Contains("tmux"))
            return true;

        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            return true;

        return false;
    }
}
