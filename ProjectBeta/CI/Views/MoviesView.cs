using ProjectBeta.CI.Components;

namespace ProjectBeta.CI.Views;

public sealed class MoviesView : Form
{
    public MoviesView()
    {
        Table("Movie", "Time", "Seats")
            .AddRow("Dune: Part Two", "19:30", 42)
            .AddRow("The Grand Budapest Hotel", "21:00", 18)
            .AddRow("Spider-Man: Across the Spider-Verse", "23:15", 6);
    }
}