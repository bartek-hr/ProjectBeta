using ProjectBeta.CI.Components;

namespace ProjectBeta.CI.Views;

public sealed class DemoView : Form
{
    public DemoView()
    {
        string? statusMessage = null;

        Heading("Component Demo");
        Label("Showcasing all available input types. Tab moves between components, Shift+Tab goes back, Escape exits.");
        Label("Arrow keys adjust numbers, move through options, and change the active button inside navigation groups. Space toggles checkboxes, toggles, and multi-select items.");
        Divider();

        Label("Text Inputs");
        TextInput("Username").Placeholder("jane_doe").Required().Min(3).Max(20);
        TextInput("Email").Placeholder("jane@example.com").Required()
            .Pattern(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", "Must be a valid email address");
        TextInput("Password").Placeholder("Choose a password").Required().Masked();
        TextInput("Bio").Placeholder("Tell us about yourself...");

        Divider();

        Label("Number Inputs");
        NumberInput("Age").Min(0).Max(150).Step(1).Required();
        NumberInput("Rating").Min(0).Max(5).Step(0.5).Precision(1);
        NumberInput("Temperature").Min(-40).Max(60).Step(0.1).Precision(1);

        Divider();

        Label("Select Input");
        Select("Favorite Color").AddOption("Red").AddOption("Green").AddOption("Blue").AddOption("Yellow").AddOption("Purple").Required();
        Select("Country").AddOption("United States").AddOption("Canada").AddOption("United Kingdom").AddOption("Germany").AddOption("Japan").AddOption("Australia");

        Divider();

        Label("Checkbox Inputs");
        Checkbox("Accept Terms").Default(false);
        Checkbox("Subscribe to newsletter").Default(true);
        Checkbox("Enable dark mode");

        Divider();

        Label("Toggle Inputs");
        Toggle("Two-factor authentication").Default(true);
        Toggle("Public profile");

        Divider();

        Label("Radio Group Input");
        RadioGroup("Contact Method").AddOption("Email").AddOption("Phone").AddOption("SMS").Required();

        Divider();

        Label("Multi-Select Input");
        MultiSelect("Interests")
            .AddOption("Movies").AddOption("Books").AddOption("Games")
            .AddOption("Travel").AddOption("Cooking").AddOption("Music")
            .Defaults("Movies", "Music")
            .Required();

        Divider();

        Label("Date Inputs");
        DateInput("Birth Date").Required().Min(new DateOnly(1900, 1, 1)).Max(DateOnly.FromDateTime(DateTime.Today));
        DateInput("Appointment Date").Default(DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

        Divider();

        Label("Table (display-only)");
        Table("Movie", "Time", "Seats") // only used to show data
            .AddRow("Dune: Part Two", "19:30", 42)
            .AddRow("The Grand Budapest Hotel", "21:00", 18)
            .AddRow("Spider-Man: Across the Spider-Verse", "23:15", 6);

        Divider();

        Label("Interactive Table (Tab to focus, arrows to navigate, Enter to select)");
        string? selectedMovie = null;
        Table<string>("Movie", "Time", "Seats") // only used to show data you can select
            .AddRow("dune", "Dune: Part Two", "19:30", 42) // first parameter is the callback object
            .AddRow("grand-budapest", "The Grand Budapest Hotel", "21:00", 18)
            .AddRow("spider-verse", "Spider-Man: Across the Spider-Verse", "23:15", 6)
            .OnSelect(id => { selectedMovie = id; }); // id = dune, grand-budapest, or spider-verse
        Message(() => selectedMovie != null ? $"Selected movie: {selectedMovie}" : null);

        Divider();

        Message(() => statusMessage);
        Navigation(
            Button("Submit").OnClick(form =>
            {
                statusMessage =
                    $"Demo submitted. Age={form.Get<double?>("Age")}, " +
                    $"Color={form.Get<string?>("Favorite Color") ?? "(none)"}, " +
                    $"Date={form.Get<DateOnly?>("Birth Date")?.ToString("yyyy-MM-dd") ?? "(none)"}."; 
            }),
            Button("Reset").OnClick(() =>
            {
                statusMessage = "Reset is not implemented in this demo.";
            }));
    }
}
