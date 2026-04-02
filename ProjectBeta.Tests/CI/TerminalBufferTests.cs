using ProjectBeta.CI.Rendering;

namespace ProjectBeta.Tests.CI;

[TestClass]
public class TerminalBufferTests
{
    [TestMethod]
    public void Write_AppendsTextToCurrentLine()
    {
        var buffer = new TerminalBuffer(40);

        buffer.Write("hello").WriteLine();

        var frame = buffer.ToFrame();
        Assert.AreEqual(1, frame.Height);
        Assert.AreEqual("hello", frame.GetRowPlainText(0));
    }

    [TestMethod]
    public void WriteLine_CreatesNewLine()
    {
        var buffer = new TerminalBuffer(40);

        buffer.WriteLine("first");
        buffer.WriteLine("second");

        var frame = buffer.ToFrame();
        Assert.AreEqual(2, frame.Height);
        Assert.AreEqual("first", frame.GetRowPlainText(0));
        Assert.AreEqual("second", frame.GetRowPlainText(1));
    }

    [TestMethod]
    public void EnsureHeight_PreservesBlankRows()
    {
        var buffer = new TerminalBuffer(10);

        buffer.EnsureHeight(3);

        var frame = buffer.ToFrame();
        Assert.AreEqual(3, frame.Height);
        Assert.AreEqual(string.Empty, frame.GetRowPlainText(0));
        Assert.AreEqual(string.Empty, frame.GetRowPlainText(1));
        Assert.AreEqual(string.Empty, frame.GetRowPlainText(2));
    }

    [TestMethod]
    public void SetCursor_PreservesCursorMetadata()
    {
        var buffer = new TerminalBuffer(10);

        buffer.SetCursor(4, 2);

        var frame = buffer.ToFrame();
        Assert.AreEqual(4, frame.CursorLeft);
        Assert.AreEqual(2, frame.CursorTop);
    }

    [TestMethod]
    public void BlankLine_InsertsEmptyRows()
    {
        var buffer = new TerminalBuffer(40);

        buffer.WriteLine("before");
        buffer.BlankLine(2);
        buffer.WriteLine("after");

        var frame = buffer.ToFrame();
        Assert.AreEqual(4, frame.Height);
        Assert.AreEqual("before", frame.GetRowPlainText(0));
        Assert.AreEqual(string.Empty, frame.GetRowPlainText(1));
        Assert.AreEqual(string.Empty, frame.GetRowPlainText(2));
        Assert.AreEqual("after", frame.GetRowPlainText(3));
    }

    [TestMethod]
    public void Repeat_WritesRepeatedCharacter()
    {
        var buffer = new TerminalBuffer(40);

        buffer.Repeat('-', 5).WriteLine();

        var frame = buffer.ToFrame();
        Assert.AreEqual("-----", frame.GetRowPlainText(0));
    }

    [TestMethod]
    public void Indent_PrependsSpacesToLines()
    {
        var buffer = new TerminalBuffer(40);

        using (buffer.Indent(4))
        {
            buffer.WriteLine("indented");
        }

        buffer.WriteLine("not indented");

        var frame = buffer.ToFrame();
        Assert.AreEqual("    indented", frame.GetRowPlainText(0));
        Assert.AreEqual("not indented", frame.GetRowPlainText(1));
    }

    [TestMethod]
    public void StyledSpans_ArePreservedInFrame()
    {
        var buffer = new TerminalBuffer(40);

        buffer.Write("normal ").Write("red", Style.Error).WriteLine();

        var frame = buffer.ToFrame();
        Assert.AreEqual("normal red", frame.GetRowPlainText(0));
        Assert.AreEqual(2, frame.Rows[0].Count);
        Assert.AreEqual(Style.Error, frame.Rows[0][1].Style);
    }
}
