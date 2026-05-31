using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public static class UI
{
    public static Form Form(IConsoleDriver? consoleDriver = null)
        => new(consoleDriver);

    public static UserRegistration UserRegistration()
        => new();

    public static InputText TextInput(string label)
        => new(label);

    public static NumberInput NumberInput(string label)
        => new(label);

    public static Select Select(string label)
        => new(label);

    public static Checkbox Checkbox(string label)
        => new(label);

    public static Button Button(string label)
        => new(label);

    public static Navigation Navigation(params Button[] buttons)
        => new(buttons);

    public static Heading Heading(string text)
        => new(text);

    public static Label Label(string text)
        => new(text);

    public static Message Message(Func<string?> messageProvider)
        => new(messageProvider);

    public static Table Table(params string[] headers)
        => new(headers);

    public static Divider Divider()
        => new();

    public static Spacer Spacer(int lines = 1)
        => new(lines);

    public static MultiSelect MultiSelect(string label)
        => new(label);

    public static Toggle Toggle(string label)
        => new(label);

    public static RadioGroup RadioGroup(string label)
        => new(label);

    public static DateInput DateInput(string label)
        => new(label);
}
