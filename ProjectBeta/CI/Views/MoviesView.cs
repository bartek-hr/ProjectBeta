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
    private int _locationId;
    private readonly BookingLogic _bookingLogic;
    private DateOnly _selectedDate;
    private IReadOnlyList<MovieSchedule> _schedule = [];
    private string? _statusMessage;
    private string _searchQuery = string.Empty;
    private User _user = null!;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;

    public MoviesView(MovieLogic movieLogic, BookingLogic bookingLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _movieLogic = movieLogic;
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
    public void SetUser(User user, int locationId)
    {
        _user = user;
        _searchQuery = string.Empty;
        _locationId = locationId;
        ClearChildren();
        LoadSchedule();
        InitializeForm();
    }
    private void LoadSchedule()
    {
        _schedule = _movieLogic.GetOrGenerateSchedule(_selectedDate);
        var today = DateOnly.FromDateTime(DateTime.Today);
        _statusMessage = _schedule.Count == 0 && _selectedDate < today
            ? l10n("movies.list.status.no_schedule_for_date")
            : null;
    }
    private void InitializeForm()
    {
        Label(l10n("movies.list.heading", new Dictionary<string, string>
        {
            ["date"] = _selectedDate.ToString("yyyy-MM-dd")
        }));
        Message(() => _statusMessage);

        var searchInput = TextInput("Search by title");
        Button("Search").OnClick(() =>
        {
            _searchQuery = searchInput.Value ?? string.Empty;
            RefreshView();
        });
        Button("Clear").OnClick(() =>
        {
            _searchQuery = string.Empty;
            RefreshView();
        });

        Divider();

        var filteredSchedule = _movieLogic.SearchSchedule(_schedule, _searchQuery);

        var table = new Table<MovieSchedule>(
            l10n("movies.list.table.movie"),
            l10n("movies.list.table.rating"),
            l10n("movies.list.table.start"),
            l10n("movies.list.table.end"),
            l10n("movies.list.table.auditorium"),
            l10n("movies.list.table.space_left")
        )
        .EmptyMessage(l10n("movies.list.empty"))
        .OnSelect(OnMovieSelected);
        foreach (var schedule in filteredSchedule)
        {
            table.AddRow(
                schedule,
                schedule.Movie.Title,
                schedule.Movie.Rating?.ToString("0.0") ?? "-",
                schedule.StartTime.ToString("HH:mm"),
                schedule.EndTime.ToString("HH:mm"),
                schedule.Auditorium.Name,
                $"{DetermineSpaceLeft(schedule)}"
            );
        }
        Add(table);
        Divider();
        Button(l10n("movies.list.actions.back")).OnClick(NavigateToMain);
    }
    private int DetermineSpaceLeft(MovieSchedule schedule)
    {
        var reservedSeats = GetReservedSeats(schedule, schedule.Auditorium.Id);
        return schedule.Auditorium.Capacity - reservedSeats.Count;
    }

    private HashSet<string> GetReservedSeats(MovieSchedule schedule, int auditoriumId)
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
        accountView.SetView(_user, schedule, schedule.Auditorium, _locationId);
        _appLoop.Display(accountView);
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

}
