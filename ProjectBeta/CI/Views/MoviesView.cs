using ProjectBeta.CI.Components;

namespace ProjectBeta.CI.Views;

public sealed class MoviesView : Form
{
    public MoviesView()
    {
        string? selectedMessage = null;

        Table<string>("ID", "Movie", "Time", "Seats")
            .AddRow("1", 1, "Dune: Part Two", "19:30", 42)
            .AddRow("2", 2, "The Grand Budapest Hotel", "21:00", 18)
            .AddRow("3", 3, "Spider-Man: Across the Spider-Verse", "23:15", 6)
            .OnSelect(id => { selectedMessage = $"Selected movie ID: {id}"; });
        Message(() => selectedMessage);
    }
}