using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public class Form : RootComponent
{
    public Form(IConsoleDriver? consoleDriver = null)
        : base(consoleDriver)
    {
    }

    public void Display()
    {
        ProjectBeta.Program.Display(this);
    }

    public new Form Add(Component child)
    {
        base.Add(child);

        if (child is Button button)
            button.ParentForm = this;

        return this;
    }

    public new Form AddRange(IEnumerable<Component> children)
    {
        foreach (var child in children)
            Add(child);

        return this;
    }

    public new Form AddRange(params Component[] children)
    {
        return AddRange(children.AsEnumerable());
    }

    public T? Get<T>(string label)
    {
        var component = Children
            .OfType<IValueComponent>()
            .FirstOrDefault(c => c.FieldKey == label)
            ?? Children
                .OfType<IValueComponent>()
                .FirstOrDefault(c => c.Label == label);

        return component?.Value is T val ? val : default;
    }

    public bool ValidateAll()
    {
        var allValid = true;

        foreach (var child in Children)
        {
            if (child is not IValidatable validatable)
                continue;

            if (child.IsHidden)
            {
                validatable.SetErrors([]);
                continue;
            }

            var errors = validatable.Validate();
            if (errors.Count > 0)
                allValid = false;
            validatable.SetErrors(errors);
        }

        return allValid;
    }

    // --- Protected builder methods for subclasses ---

    protected InputText TextInput(string label)
    {
        var c = new InputText(label);
        Add(c);
        return c;
    }

    protected NumberInput NumberInput(string label)
    {
        var c = new NumberInput(label);
        Add(c);
        return c;
    }

    protected Select Select(string label)
    {
        var c = new Select(label);
        Add(c);
        return c;
    }

    protected MultiSelect MultiSelect(string label)
    {
        var c = new MultiSelect(label);
        Add(c);
        return c;
    }

    protected Checkbox Checkbox(string label)
    {
        var c = new Checkbox(label);
        Add(c);
        return c;
    }

    protected Toggle Toggle(string label)
    {
        var c = new Toggle(label);
        Add(c);
        return c;
    }

    protected RadioGroup RadioGroup(string label)
    {
        var c = new RadioGroup(label);
        Add(c);
        return c;
    }

    protected DateInput DateInput(string label)
    {
        var c = new DateInput(label);
        Add(c);
        return c;
    }

    protected Button Button(string label)
    {
        var c = new Button(label);
        Add(c);
        return c;
    }

    protected Heading Heading(string text)
    {
        var c = new Heading(text);
        Add(c);
        return c;
    }

    protected Label Label(string text)
    {
        var c = new Label(text);
        Add(c);
        return c;
    }

    protected Message Message(Func<string?> messageProvider)
    {
        var c = new Message(messageProvider);
        Add(c);
        return c;
    }

    protected Table Table(params string[] headers)
    {
        var c = new Table(headers);
        Add(c);
        return c;
    }

    protected Table<T> Table<T>(params string[] headers) where T : class
    {
        var c = new Table<T>(headers);
        Add(c);
        return c;
    }

    protected Divider Divider()
    {
        var c = new Divider();
        Add(c);
        return c;
    }

    protected Spacer Spacer(int lines = 1)
    {
        var c = new Spacer(lines);
        Add(c);
        return c;
    }

    protected LogoutButton LogoutButton(AppLoop appLoop, IServiceProvider serviceProvider)
    {
        var c = new LogoutButton(appLoop, serviceProvider);
        Add(c);
        return c;
    }
}
