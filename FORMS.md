# Console Forms

Declarative console form builder. Extend `Form`, call component methods in the constructor.

```csharp
public class LoginView : Form
{
    public LoginView()
    {
        Heading("Login");
        TextInput("Username").Required();
        TextInput("Password").Required().Masked();
        Button("Log in").OnClick(form =>
        {
            var user = form.Get<string>("Username");
            var pass = form.Get<string>("Password");
            
            // authenticate user
        });
    }
}
```

`Tab` / `Shift+Tab` navigates. `Escape` exits.

---

## Components

- Input
  - [TextInput](#textinput)
  - [NumberInput](#numberinput)
  - [DateInput](#dateinput)
- Selection
  - [Select](#select)
  - [MultiSelect](#multiselect)
  - [RadioGroup](#radiogroup)
- Toggle
  - [Checkbox](#checkbox)
  - [Toggle](#toggle)
- Action
  - [Button](#button)
- Display
  - [Heading](#heading)
  - [Label](#label)
  - [Divider](#divider)
  - [Spacer](#spacer)
  - [Message](#message)

All components have:

```
.Hidden(bool)                    // hide from view, focus, and validation
.Hidden(() => bool)              // conditionally hide, re-evaluated each frame
```

---

### TextInput

```
TextInput(string label)
    .Required()
    .Masked()                    // display as ****
    .Placeholder(string)
    .Min(int)                    // min length
    .Max(int)                    // max length
    .Pattern(string, string?)    // regex validation, optional error message
    .Validator(value => string?) // custom validator, return error or null
```

Keys: type, `Backspace`, `Delete`, `Left`/`Right`, `Home`/`End`
Value: `string`

### NumberInput

```
NumberInput(string label)
    .Required()
    .Min(double)
    .Max(double)
    .Step(double)                // arrow key increment (default 1)
    .Precision(int)              // decimal places
```

Keys: `Up`/`Down` to increment/decrement, type digits
Value: `double?`

### DateInput

```
DateInput(string label)
    .Required()
    .Default(DateOnly)
    .Min(DateOnly)
    .Max(DateOnly)
```

Keys: `Left`/`Right` between year/month/day, `Up`/`Down` to adjust, type digits
Value: `DateOnly?`

### Select

Single selection, collapses when unfocused.

```
Select(string label)
    .AddOption(string)
    .Required()
```

Keys: `Up`/`Down`, `Enter`/`Space` to confirm
Value: `string?`

### MultiSelect

Multiple selections with checkboxes.

```
MultiSelect(string label)
    .AddOption(string)
    .Defaults(params string[])   // pre-selected options
    .Required()                  // at least one
```

Keys: `Up`/`Down`, `Space` to toggle
Value: `string[]`

### RadioGroup

Single selection, always expanded.

```
RadioGroup(string label)
    .AddOption(string)
    .Required()
```

Keys: `Up`/`Down`, `Enter`/`Space` to select
Value: `string?`

### Checkbox

```
Checkbox(string label)
    .Default(bool)
```

Keys: `Space`
Value: `bool`

### Toggle

On/Off switch.

```
Toggle(string label)
    .Default(bool)
```

Keys: `Space`/`Enter`
Value: `bool`

### Button

```
Button(string label)
    .OnClick(Action)
    .OnClick(Action<Form>)       // auto-validates before calling
```

Keys: `Enter`/`Space`

### Heading

```
Heading(string text)             // bold title
```

### Label

```
Label(string text)               // muted text
```

### Divider

```
Divider()                        // horizontal line
```

### Spacer

```
Spacer(int lines = 1)            // blank lines
```

### Message

```
Message(() => string?)           // dynamic text, hidden when null/empty
```

---

## Form

```
Form
    .Get<T>(string label)        // get a component's value by label
    .ValidateAll()               // validate all visible inputs, returns bool
    .Display()                   // show this form via Program.Display(this)
    .Close()                     // close this form and restore the previous view
```

---

## Use Cases

### Reading values on submit

```csharp
public class ContactForm : Form
{
    public ContactForm()
    {
        TextInput("Name").Required();
        TextInput("Email").Required().Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Invalid email");

        Button("Send").OnClick(form =>
        {
            var name = form.Get<string>("Name");
            var email = form.Get<string>("Email");
            
            // save to database
        });
    }
}
```

### Conditional visibility

Show a field only when another field has a specific value:

```csharp
public class OrderForm : Form
{
    public OrderForm()
    {
        var method = RadioGroup("Delivery")
            .AddOption("Pickup")
            .AddOption("Ship");

        // hides 'address' field when 'pickup' is selected
        TextInput("Address")
            .Required()
            .Hidden(() => method.Value != "Ship");
    }
}
```

### Referencing values of other components

```csharp
var password = TextInput("Password").Required().Masked();
TextInput("Confirm Password").Required().Masked()
    .Validator(val => val != password.Value ? "Passwords must match" : null);
```
