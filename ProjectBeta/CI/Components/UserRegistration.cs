namespace ProjectBeta.CI.Components;

public sealed class UserRegistration : Form
{
    public UserRegistration()
    {
        string? statusMessage = null;

        Heading(l10n("demo.user_registration.heading"));
        Label(l10n("demo.user_registration.instructions"));
        Divider();
        TextInput(l10n("demo.user_registration.fields.username.label")).Key("username").Placeholder(l10n("demo.user_registration.fields.username.placeholder")).Required().Min(3);
        TextInput(l10n("demo.user_registration.fields.fullname.label")).Key("fullname").Placeholder(l10n("demo.user_registration.fields.fullname.placeholder")).Required();
        TextInput(l10n("demo.user_registration.fields.email.label")).Key("email").Placeholder(l10n("demo.user_registration.fields.email.placeholder")).Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", l10n("demo.user_registration.validation.email_invalid"));
        var password = TextInput(l10n("demo.user_registration.fields.password.label")).Key("password").Placeholder(l10n("demo.user_registration.fields.password.placeholder")).Required().Masked();
        TextInput(l10n("demo.user_registration.fields.confirm_password.label")).Key("confirm_password").Placeholder(l10n("demo.user_registration.fields.confirm_password.placeholder")).Required().Masked()
            .Validator(val => val != password.Value ? l10n("demo.user_registration.validation.password_mismatch") : null);
        TextInput(l10n("demo.user_registration.fields.age.label")).Key("age").Placeholder(l10n("demo.user_registration.fields.age.placeholder")).Required()
            .Validator(val => !int.TryParse(val, out var n) || n <= 0 ? l10n("demo.user_registration.validation.age_positive") : null);
        Divider();
        Message(() => statusMessage);
        Button(l10n("demo.user_registration.actions.register")).OnClick(form =>
        {
            statusMessage = l10n("demo.user_registration.status.registered", new Dictionary<string, string>
            {
                ["name"] = form.Get<string>("fullname") ?? string.Empty,
                ["username"] = form.Get<string>("username") ?? string.Empty,
                ["email"] = form.Get<string>("email") ?? string.Empty,
                ["age"] = form.Get<string>("age") ?? string.Empty
            });
        });
    }
}
