using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class MoviesView : Form
{
    private readonly MovieLogic _movieLogic;
    private DateOnly _selectedDate;
    private IReadOnlyList<MovieSchedule> _schedule = [];
    private string? _statusMessage;

    public MoviesView(MovieLogic movieLogic)
    {
        _movieLogic = movieLogic;
        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        LoadSchedule();
        InitializeForm();
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.LeftArrow)
        {
            _selectedDate = _selectedDate.AddDays(-1);
            RefreshView();
            return true;
        }

        if (key.Key == ConsoleKey.RightArrow)
        {
            _selectedDate = _selectedDate.AddDays(1);
            RefreshView();
            return true;
        }

        return base.ProcessKey(key);
    }

    private void RefreshView()
    {
        LoadSchedule();
        ClearChildren();
        InitializeForm();
    }

    private void LoadSchedule()
    {
        _schedule = _movieLogic.GetOrGenerateSchedule(_selectedDate);
        var today = DateOnly.FromDateTime(DateTime.Today);
        _statusMessage = _schedule.Count == 0 && _selectedDate < today
            ? "No schedule for this date."
            : null;
    }

    private void InitializeForm()
    {
        Label($"Movies for date: {_selectedDate:yyyy-MM-dd}");
        Message(() => _statusMessage);

        var table = Table("Movie", "Rating", "Start", "End", "Auditorium", "Space Left")
            .EmptyMessage("No movies scheduled.");

        foreach (var schedule in _schedule)
        {
            table.AddRow(
                schedule.Movie.Title,
                schedule.Movie.Rating?.ToString("0.0") ?? "-",
                schedule.StartTime.ToString("HH:mm"),
                schedule.EndTime.ToString("HH:mm"),
                "?",
                "?");
        }
    }
}
