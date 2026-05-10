using ProjectBeta.CI;
using ProjectBeta.CI.Views;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace ProjectBeta.CI.Views;

public sealed class MoviesView : Form
{
    private readonly MovieLogic _movieLogic;
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly BookingLogic _bookingLogic;
    private DateOnly _selectedDate;
    private IReadOnlyList<MovieSchedule> _schedule = [];
    private string? _statusMessage;
    private User _user = null!;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private int _auditoriumId;

    public MoviesView(MovieLogic movieLogic, AuditoriumLogic auditoriumLogic, BookingLogic bookingLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _movieLogic = movieLogic;
        _auditoriumLogic = auditoriumLogic;
        _bookingLogic = bookingLogic;
        _selectedDate = DateOnly.FromDateTime(DateTime.Today);
        _serviceProvider = serviceProvider;
        _user = new User();
        _appLoop = appLoop;
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
    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        LoadSchedule();  
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

        var table = new Table<MovieSchedule>(
            "Movie", "Rating", "Start", "End", "Auditorium", "Space Left"
        )
        .EmptyMessage("No movies scheduled.")
        .OnSelect(OnMovieSelected);
        _auditoriumId = 3;
        foreach (var schedule in _schedule)
        {
            table.AddRow(
                schedule,
                schedule.Movie.Title,
                schedule.Movie.Rating?.ToString("0.0") ?? "-",
                schedule.StartTime.ToString("HH:mm"),
                schedule.EndTime.ToString("HH:mm"),
                $"{_auditoriumId}",
                $"{determineSpaceLeft(_auditoriumId, schedule)}"
            );
        }
        Add(table);

    }
    private int determineSpaceLeft(int auditoriumId, MovieSchedule schedule)
    {
        Auditorium _auditorium = _auditoriumLogic.GetById(auditoriumId);
        var _reservedSeats = GetReservedSeats(schedule, _auditorium.Id);
        return _auditorium.Capacity - _reservedSeats.Count;
        

    }

    private HashSet<string>? GetReservedSeats(MovieSchedule schedule, int auditoriumId)
    {
        List<Booking> reservations = _bookingLogic.GetBookingsByCreatedAtAndAuditoriumID(schedule.ScheduleDate.ToDateTime(schedule.StartTime), auditoriumId);
        return reservations
            .Where(b => !string.IsNullOrWhiteSpace(b.Seats))
            .SelectMany(b => b.Seats.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private void OnMovieSelected(MovieSchedule schedule)
    {
            Console.Clear();
            var accountView = _serviceProvider.GetRequiredService<MovieSeatBookingView>();
            accountView.SetView(_user, schedule, _auditoriumId);
            _appLoop.Display(accountView);
    }
    
}