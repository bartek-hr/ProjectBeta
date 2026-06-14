using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class MovieCreatorView : Form
{
    private readonly MovieAccess _movieAccess;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = new();
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public MovieCreatorView(MovieAccess movieAccess, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _movieAccess = movieAccess;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("admin.movies.creator.heading"));
        Divider();

        Message(() => GetError("general"));

        TextInput(l10n("admin.movies.creator.fields.title.label")).Key("title").Required().Min(1).Max(200);
        Message(() => GetError("title"));

        TextInput(l10n("admin.movies.creator.fields.description.label")).Key("description").Max(2000);
        Message(() => GetError("description"));

        TextInput(l10n("admin.movies.creator.fields.genres.label")).Key("genres").Max(500);
        Message(() => GetError("genres"));

        TextInput(l10n("admin.movies.creator.fields.rating.label")).Key("rating").Max(10);
        Message(() => GetError("rating"));

        TextInput(l10n("admin.movies.creator.fields.runtime.label")).Key("runtime").Max(10);
        Message(() => GetError("runtime"));

        Divider();
        Message(() => _statusMessage);
        Navigation(
            Button(l10n("admin.movies.creator.actions.save")).OnClick(OnSubmit),
            Button(l10n("admin.movies.creator.actions.cancel")).OnClick(NavigateBack));
    }

    private string? GetError(string key)
    {
        return _fieldErrors != null && _fieldErrors.TryGetValue(key, out var errors)
            ? string.Join("\n", errors)
            : null;
    }

    private void OnSubmit(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var title = form.Get<string>("title");
        if (string.IsNullOrWhiteSpace(title))
        {
            _fieldErrors = new Dictionary<string, string[]> { ["title"] = [l10n("admin.movies.creator.errors.title_required")] };
            Invalidate();
            return;
        }

        var genresRaw = form.Get<string>("genres") ?? string.Empty;
        var genres = genresRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        double? rating = null;
        var ratingStr = form.Get<string>("rating");
        if (!string.IsNullOrWhiteSpace(ratingStr))
        {
            if (!double.TryParse(ratingStr, out var parsedRating) || parsedRating < 0 || parsedRating > 10)
            {
                _fieldErrors = new Dictionary<string, string[]> { ["rating"] = [l10n("admin.movies.creator.errors.rating_invalid")] };
                Invalidate();
                return;
            }
            rating = parsedRating;
        }

        int? runtimeSeconds = null;
        var runtimeStr = form.Get<string>("runtime");
        if (!string.IsNullOrWhiteSpace(runtimeStr))
        {
            if (!int.TryParse(runtimeStr, out var parsedMinutes) || parsedMinutes <= 0)
            {
                _fieldErrors = new Dictionary<string, string[]> { ["runtime"] = [l10n("admin.movies.creator.errors.runtime_invalid")] };
                Invalidate();
                return;
            }
            runtimeSeconds = parsedMinutes * 60;
        }

        var movie = new Movie
        {
            Title = title!,
            Description = form.Get<string>("description") ?? string.Empty,
            Genres = genres,
            Rating = rating,
            RuntimeSeconds = runtimeSeconds
        };

        _movieAccess.Add(movie);

        Console.Clear();
        var manageView = _serviceProvider.GetRequiredService<ManageMoviesView>();
        manageView.SetUser(_user, l10n("admin.movies.creator.status.added"));
        _appLoop.Display(manageView);
    }

    private void NavigateBack()
    {
        Console.Clear();
        var manageView = _serviceProvider.GetRequiredService<ManageMoviesView>();
        manageView.SetUser(_user);
        _appLoop.Display(manageView);
    }
}
