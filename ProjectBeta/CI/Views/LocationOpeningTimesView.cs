using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationOpeningTimesView : Form
{
    private const string TimePattern = "^([01]\\d|2[0-3]):[0-5]\\d$";

    private readonly LocationOpeningTimeLogic _openingTimeLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location _location = null!;
    private string? _statusMessage;

    public LocationOpeningTimesView(
        LocationOpeningTimeLogic openingTimeLogic,
        AppLoop appLoop,
        IServiceProvider serviceProvider)
    {
        _openingTimeLogic = openingTimeLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetContext(User user, Location location)
    {
        _user = user;
        _location = location;
        ClearChildren();
        InitializeForm();
    }

    private void RefreshView()
    {
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("admin.locations.opening_times.heading", new Dictionary<string, string> { ["location"] = _location.Name }));
        Message(() => _statusMessage);

        var rules = _openingTimeLogic.GetByLocationId(_location.Id);
        var table = new Table<LocationOpeningTime>(
            "Start date",
            "Expires at",
            "Opening",
            "Closing",
            "Created at"
        )
        .EmptyMessage("No opening-time rules found.");

        foreach (var rule in rules)
        {
            table.AddRow(
                rule,
                FormatStartDate(rule.StartDate),
                FormatExpiresAt(rule.ExpiresAt),
                FormatTime(rule.OpeningTime),
                FormatTime(rule.ClosingTime),
                FormatCreatedAt(rule.CreatedAt)
            );
        }

        Add(table);
        Divider();

        DateInput("Start date").Key("startDate").Required().Default(DateOnly.FromDateTime(DateTime.Today));
        var noExpirationInput = Checkbox("No expiration").Key("noExpiration").Default(true);
        DateInput("Expires at")
            .Key("expiresAt")
            .Required()
            .Default(DateOnly.FromDateTime(DateTime.Today))
            .Hidden(() => noExpirationInput.Value);
        var closedInput = Checkbox("Closed").Key("closed");
        TextInput("Opening time")
            .Key("openingTime")
            .Required()
            .Pattern(TimePattern, "Use HH:mm, for example 09:00.")
            .Default(LocationOpeningTimeLogic.DefaultOpeningTime)
            .Hidden(() => closedInput.Value);
        TextInput("Closing time")
            .Key("closingTime")
            .Required()
            .Pattern(TimePattern, "Use HH:mm, for example 20:00.")
            .Default(LocationOpeningTimeLogic.DefaultClosingTime)
            .Hidden(() => closedInput.Value);

        Divider();

        Navigation(
            Button("Save").OnClick(form =>
            {
                var closed = form.Get<bool>("closed");
                var noExpiration = form.Get<bool>("noExpiration");
                var startDate = form.Get<DateOnly?>("startDate")!.Value;
                var expiresAt = noExpiration ? DateOnly.MaxValue : form.Get<DateOnly?>("expiresAt")!.Value;

                TimeOnly? openingTime = closed ? null : ParseTime(form.Get<string>("openingTime"));
                TimeOnly? closingTime = closed ? null : ParseTime(form.Get<string>("closingTime"));

                try
                {
                    _openingTimeLogic.Add(new LocationOpeningTime
                    {
                        LocationId = _location.Id,
                        StartDate = startDate,
                        ExpiresAt = expiresAt,
                        OpeningTime = openingTime,
                        ClosingTime = closingTime
                    }, _user);

                    _statusMessage = "Opening-time rule saved. Matching generated schedules were invalidated.";
                    RefreshView();
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or FormatException)
                {
                    _statusMessage = ex.Message;
                }
            }),
            Button("Back").OnClick(() =>
            {
                Console.Clear();
                var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
                detailView.SetView(_user, _location);
                _appLoop.Display(detailView);
            }));
    }

    private static string FormatTime(TimeOnly? time)
    {
        return time?.ToString("HH:mm") ?? "-";
    }

    private static string FormatStartDate(DateOnly date)
    {
        return date == DateOnly.MinValue
            ? l10n("admin.locations.opening_times.values.always")
            : date.ToString("yyyy-MM-dd");
    }

    private static string FormatExpiresAt(DateOnly date)
    {
        return date == DateOnly.MaxValue
            ? l10n("admin.locations.opening_times.values.never")
            : date.ToString("yyyy-MM-dd");
    }

    private static string FormatCreatedAt(DateTime createdAt)
    {
        return createdAt == DateTime.MinValue
            ? "Default"
            : createdAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    private static TimeOnly ParseTime(string? value)
    {
        if (TimeOnly.TryParseExact(value, "HH:mm", out var time))
        {
            return time;
        }

        throw new FormatException("Use HH:mm for opening and closing times.");
    }
}
