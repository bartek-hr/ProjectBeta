namespace ProjectBeta.CI.Components;

public sealed class UserRegistration : Form
{
    public UserRegistration()
    {
        string? statusMessage = null;

        Heading("Register");
        Label("Demo registration form. Tab moves focus, Enter activates the button, Escape exits.");
        Divider();
        TextInput("Username").Placeholder("jane_doe").Required().Min(3);
        TextInput("Fullname").Placeholder("Jane Doe").Required();
        TextInput("Email").Placeholder("jane@example.com").Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Email is not valid");
        var password = TextInput("Password").Placeholder("Choose a password").Required().Masked();
        TextInput("Confirm Password").Placeholder("Repeat the password").Required().Masked()
            .Validator(val => val != password.Value ? "Passwords do not match" : null);
        TextInput("Age").Placeholder("18").Required()
            .Validator(val => !int.TryParse(val, out var n) || n <= 0 ? "Age must be a positive number" : null);
        Divider();
        Message(() => statusMessage);
        Button("Register").OnClick(form =>
        {
            var user = form.Get<string>("Username");
            var name = form.Get<string>("Fullname");
            var email = form.Get<string>("Email");
            var ageVal = form.Get<string>("Age");

            statusMessage = $"Demo only: registered {name} ({user}) with email {email}, age {ageVal}.";
        });
    }
}
