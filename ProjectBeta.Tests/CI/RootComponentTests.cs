using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.CI.Rendering;

namespace ProjectBeta.Tests.CI;

[TestClass]
public class RootComponentTests
{
    [TestMethod]
    public void Render_DoesNotRewriteUnchangedFrame()
    {
        var console = new FakeConsoleDriver();
        var root = new Form(console);
        var text = "Hello";

        root.Add(new DynamicTextComponent(() => text));

        root.Render();
        Assert.AreEqual(1, console.WriteCalls.Count);
        Assert.AreEqual(new WriteCall(0, 0, "Hello"), console.WriteCalls[0]);

        console.ClearWrites();
        root.Render();

        Assert.AreEqual(0, console.WriteCalls.Count);
    }

    [TestMethod]
    public void Render_RewritesOnlyChangedCharacters()
    {
        var console = new FakeConsoleDriver();
        var root = new Form(console);
        var text = "Hello";

        root.Add(new DynamicTextComponent(() => text));
        root.Render();

        text = "Hallo";
        console.ClearWrites();
        root.Render();

        Assert.AreEqual(1, console.WriteCalls.Count);
        Assert.AreEqual(new WriteCall(1, 0, "a"), console.WriteCalls[0]);
    }

    [TestMethod]
    public void Render_ClearsStaleCharactersWhenLineBecomesShorter()
    {
        var console = new FakeConsoleDriver();
        var root = new Form(console);
        var text = "Hello";

        root.Add(new DynamicTextComponent(() => text));
        root.Render();

        text = "Hi";
        console.ClearWrites();
        root.Render();

        Assert.AreEqual(1, console.WriteCalls.Count);
        Assert.AreEqual(new WriteCall(1, 0, "i   "), console.WriteCalls[0]);
    }

    [TestMethod]
    public void Render_ClearsRemovedRows()
    {
        var console = new FakeConsoleDriver();
        var root = new Form(console);
        var showSecondLine = true;

        root.Add(new DynamicTextComponent(() => "Row 1"));
        root.Add(new ConditionalComponent(() => showSecondLine, "Row 2"));
        root.Render();

        showSecondLine = false;
        console.ClearWrites();
        root.Render();

        Assert.AreEqual(2, console.WriteCalls.Count);
        Assert.AreEqual(new WriteCall(0, 1, "   "), console.WriteCalls[0]);
        Assert.AreEqual(new WriteCall(4, 1, " "), console.WriteCalls[1]);
    }

    [TestMethod]
    public void ProcessKey_TabMovesFocusBetweenFocusableChildren()
    {
        var root = new Form(new FakeConsoleDriver());
        var first = new InputText("First");
        var second = new InputText("Second");

        root.Add(first).Add(second);

        var changed = root.ProcessKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));

        Assert.IsTrue(changed);
        Assert.IsFalse(first.IsFocused);
        Assert.IsTrue(second.IsFocused);
    }

    [TestMethod]
    public void ProcessKey_ShiftTabMovesFocusBackwards()
    {
        var root = new Form(new FakeConsoleDriver());
        var first = new InputText("First");
        var second = new InputText("Second");

        root.Add(first).Add(second);
        root.ProcessKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));

        var changed = root.ProcessKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, true, false, false));

        Assert.IsTrue(changed);
        Assert.IsTrue(first.IsFocused);
        Assert.IsFalse(second.IsFocused);
    }

    [TestMethod]
    public void ProcessKey_ForwardsKeysOnlyToFocusedChild()
    {
        var root = new Form(new FakeConsoleDriver());
        var first = new InputText("First");
        var second = new InputText("Second");

        root.Add(first).Add(second);
        root.ProcessKey(new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false));

        Assert.AreEqual("A", first.Value);
        Assert.AreEqual(string.Empty, second.Value);

        root.ProcessKey(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        root.ProcessKey(new ConsoleKeyInfo('B', ConsoleKey.B, false, false, false));

        Assert.AreEqual("A", first.Value);
        Assert.AreEqual("B", second.Value);
    }

    [TestMethod]
    public void ProcessKey_ReturnsFalseWhenFocusedChildIgnoresKey()
    {
        var root = new Form(new FakeConsoleDriver());
        root.Add(new InputText("First"));

        var changed = root.ProcessKey(new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false));

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void ProcessKey_AllowsTypingAgainAfterClearingInput()
    {
        var root = new Form(new FakeConsoleDriver());
        var input = new InputText("First");
        root.Add(input);

        root.ProcessKey(new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false));
        root.ProcessKey(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        var changed = root.ProcessKey(new ConsoleKeyInfo('B', ConsoleKey.B, false, false, false));

        Assert.IsTrue(changed);
        Assert.AreEqual("B", input.Value);
    }

    [TestMethod]
    public void Render_RewritesRowWhenStyleChanges()
    {
        var console = new FakeConsoleDriver();
        var root = new Form(console);
        var style = Style.Default;
        var styleRef = new StyledTextComponent("Same", () => style);

        root.Add(styleRef);

        root.Render();
        console.ClearWrites();

        // Change only the style, not the text
        style = Style.Error;
        root.Render();

        // The diff should detect the style change and rewrite
        Assert.IsTrue(console.WriteCalls.Count > 0, "Style-only change should trigger a rewrite");
    }

    [TestMethod]
    public void Select_ExposesSelectedOptionThroughValue()
    {
        var input = new Select("Choice")
            .AddOption("A")
            .AddOption("B");

        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false));

        Assert.AreEqual("B", input.Value);
    }

    [TestMethod]
    public void RadioGroup_ExposesSelectedOptionThroughValue()
    {
        var input = new RadioGroup("Choice")
            .AddOption("A")
            .AddOption("B");

        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false));

        Assert.AreEqual("B", input.Value);
    }

    [TestMethod]
    public void MultiSelect_ExposesSelectedOptionsThroughValue()
    {
        var input = new MultiSelect("Choices")
            .AddOption("A")
            .AddOption("B")
            .AddOption("C");

        input.ProcessKey(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        CollectionAssert.AreEqual(new[] { "A", "B" }, input.Value);
    }

    [TestMethod]
    public void Checkbox_ExposesCheckedStateThroughValue()
    {
        var input = new Checkbox("Enabled");

        input.ProcessKey(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        Assert.IsTrue(input.Value);
    }

    [TestMethod]
    public void FormGet_UsesStableFieldKeyWhenProvided()
    {
        var form = new Form(new FakeConsoleDriver());
        var input = new InputText("Localized Username").Key("username");
        input.Value = "alice";
        form.Add(input);

        Assert.AreEqual("alice", form.Get<string>("username"));
    }

    [TestMethod]
    public void FormGet_FallsBackToLabelForLegacyCallSites()
    {
        var form = new Form(new FakeConsoleDriver());
        var input = new InputText("Username");
        input.Value = "alice";
        form.Add(input);

        Assert.AreEqual("alice", form.Get<string>("Username"));
    }

    [TestMethod]
    public void Close_RestoresPreviouslyDisplayedView()
    {
        var app = new AppLoop();
        var first = new Form(new FakeConsoleDriver());
        var second = new Form(new FakeConsoleDriver());

        app.Display(first);
        app.Display(second);

        second.Close();

        Assert.AreSame(first, app.ActiveInterface);
    }

    [TestMethod]
    public void Display_ReopensClosedView()
    {
        var app = new AppLoop();
        var form = new Form(new FakeConsoleDriver());

        form.Close();
        app.Display(form);

        Assert.IsFalse(form.IsClosed);
        Assert.AreSame(form, app.ActiveInterface);
    }

    private sealed class StyledTextComponent : Component
    {
        private readonly string _text;
        private readonly Func<Style> _styleProvider;

        public StyledTextComponent(string text, Func<Style> styleProvider)
        {
            _text = text;
            _styleProvider = styleProvider;
        }

        public override int Render(ComponentRenderContext context)
        {
            context.Buffer.WriteLine(_text, _styleProvider());
            return 1;
        }
    }

    private sealed class DynamicTextComponent : Component
    {
        private readonly Func<string> _textProvider;

        public DynamicTextComponent(Func<string> textProvider)
        {
            _textProvider = textProvider;
        }

        public override int Render(ComponentRenderContext context)
        {
            context.Buffer.WriteLine(_textProvider());
            return 1;
        }
    }

    private sealed class ConditionalComponent : Component
    {
        private readonly Func<bool> _isVisible;
        private readonly string _text;

        public ConditionalComponent(Func<bool> isVisible, string text)
        {
            _isVisible = isVisible;
            _text = text;
        }

        public override int Render(ComponentRenderContext context)
        {
            if (!_isVisible())
            {
                return 0;
            }

            context.Buffer.WriteLine(_text);
            return 1;
        }
    }

    private sealed class FakeConsoleDriver : IConsoleDriver
    {
        public List<WriteCall> WriteCalls { get; } = [];
        public int CursorLeft { get; private set; }
        public int CursorTop { get; private set; }

        public int WindowWidth { get; set; } = 80;
        public int WindowHeight { get; set; } = 25;
        public bool CursorVisible { private get; set; }

        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public void Write(string text)
        {
            WriteCalls.Add(new WriteCall(CursorLeft, CursorTop, text));
        }

        public void WriteStyled(ReadOnlySpan<Span> spans)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var span in spans)
                sb.Append(span.Text);
            WriteCalls.Add(new WriteCall(CursorLeft, CursorTop, sb.ToString()));
        }

        public void Clear()
        {
        }

        public void ClearWrites()
        {
            WriteCalls.Clear();
        }
    }

    private readonly record struct WriteCall(int Left, int Top, string Text);
}
