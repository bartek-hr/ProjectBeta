using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class CinemaOpeningTimesView : Form
{
    private const string TimePattern = "^([01]\\d|2[0-3]):[0-5]\\d$";

    private readonly CinemaOpeningTimeLogic _openingTimeLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Cinema _cinema = null!;
    private string? _statusMessage;

    public CinemaOpeningTimesView(
        CinemaOpeningTimeLogic openingTimeLogic,
        AppLoop appLoop,
        IServiceProvider serviceProvider)
    {
        _openingTimeLogic = openingTimeLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetContext(User user, Cinema cinema)
    {
        _user = user;
        _cinema = cinema;
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
        Heading(l10n("admin.cinemas.opening_times.heading", new Dictionary<string, string> { ["cinema"] = _cinema.Name }));
        Message(() => _statusMessage);

        var rules = _openingTimeLogic.GetByCinemaId(_cinema.Id);
        var table = new Table<CinemaOpeningTime>(
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
            .Default(CinemaOpeningTimeLogic.DefaultOpeningTime)
            .Hidden(() => closedInput.Value);
        TextInput("Closing time")
            .Key("closingTime")
            .Required()
            .Pattern(TimePattern, "Use HH:mm, for example 20:00.")
            .Default(CinemaOpeningTimeLogic.DefaultClosingTime)
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
                    _openingTimeLogic.Add(new CinemaOpeningTime
                    {
                        CinemaId = _cinema.Id,
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
                var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
                detailView.SetContext(_user, _cinema);
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
            ? l10n("admin.cinemas.opening_times.values.always")
            : date.ToString("yyyy-MM-dd");
    }

    private static string FormatExpiresAt(DateOnly date)
    {
        return date == DateOnly.MaxValue
            ? l10n("admin.cinemas.opening_times.values.never")
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
