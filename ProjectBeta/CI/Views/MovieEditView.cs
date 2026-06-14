using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class MovieEditView : Form
{
    private readonly MovieAccess _movieAccess;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = new();
    private Movie _movie = new();
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Dictionary<string, string[]>? _fieldErrors;
    private Action? _onBack;

    public MovieEditView(MovieAccess movieAccess, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _movieAccess = movieAccess;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Movie movie, Action? onBack = null)
    {
        _user = user;
        _movie = movie;
        _onBack = onBack;
        _confirmingDelete = false;
        _statusMessage = null;
        _fieldErrors = null;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("admin.movies.editor.heading"));
        Divider();

        Message(() => GetError("general"));

        var runtimeMinutes = _movie.RuntimeSeconds.HasValue
            ? (_movie.RuntimeSeconds.Value / 60).ToString()
            : string.Empty;

        TextInput(l10n("admin.movies.creator.fields.title.label")).Key("title").Required().Min(1).Max(200).Default(_movie.Title);
        Message(() => GetError("title"));

        TextInput(l10n("admin.movies.creator.fields.description.label")).Key("description").Max(2000).Default(_movie.Description);
        Message(() => GetError("description"));

        TextInput(l10n("admin.movies.creator.fields.genres.label")).Key("genres").Max(500)
            .Default(string.Join(", ", _movie.Genres));
        Message(() => GetError("genres"));

        TextInput(l10n("admin.movies.creator.fields.rating.label")).Key("rating").Max(10)
            .Default(_movie.Rating.HasValue ? _movie.Rating.Value.ToString("0.0") : string.Empty);
        Message(() => GetError("rating"));

        TextInput(l10n("admin.movies.creator.fields.runtime.label")).Key("runtime").Max(10).Default(runtimeMinutes);
        Message(() => GetError("runtime"));

        Divider();
        Message(() => _statusMessage);

        var saveButton = Button(l10n("admin.movies.editor.actions.save")).OnClick(OnSave);
        saveButton.Hidden(() => _confirmingDelete);

        var deleteButton = Button(l10n("admin.movies.editor.actions.delete")).OnClick(() =>
        {
            _confirmingDelete = true;
            _statusMessage = l10n("admin.movies.editor.status.confirm_delete",
                new Dictionary<string, string> { ["title"] = _movie.Title });
            Invalidate();
        });
        deleteButton.Hidden(() => _confirmingDelete);

        var backButton = Button(l10n("admin.movies.editor.actions.back")).OnClick(NavigateBack);
        backButton.Hidden(() => _confirmingDelete);

        var confirmYes = Button(l10n("admin.movies.editor.actions.confirm_delete")).OnClick(OnDeleteConfirmed);
        confirmYes.Hidden(() => !_confirmingDelete);

        var confirmNo = Button(l10n("admin.movies.editor.actions.cancel_delete")).OnClick(() =>
        {
            _confirmingDelete = false;
            _statusMessage = null;
            Invalidate();
        });
        confirmNo.Hidden(() => !_confirmingDelete);

        Navigation(saveButton, deleteButton, backButton, confirmYes, confirmNo);
    }

    private string? GetError(string key)
    {
        return _fieldErrors != null && _fieldErrors.TryGetValue(key, out var errors)
            ? string.Join("\n", errors)
            : null;
    }

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var title = form.Get<string>("title");
        if (string.IsNullOrWhiteSpace(title))
        {
            _fieldErrors = new Dictionary<string, string[]> { ["title"] = [l10n("admin.movies.editor.errors.title_required")] };
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
                _fieldErrors = new Dictionary<string, string[]> { ["rating"] = [l10n("admin.movies.editor.errors.rating_invalid")] };
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
                _fieldErrors = new Dictionary<string, string[]> { ["runtime"] = [l10n("admin.movies.editor.errors.runtime_invalid")] };
                Invalidate();
                return;
            }
            runtimeSeconds = parsedMinutes * 60;
        }

        _movie.Title = title!;
        _movie.Description = form.Get<string>("description") ?? string.Empty;
        _movie.Genres = genres;
        _movie.Rating = rating;
        _movie.RuntimeSeconds = runtimeSeconds;

        _movieAccess.Update(_movie);

        Console.Clear();
        if (_onBack != null) { _onBack(); return; }
        var manageView = _serviceProvider.GetRequiredService<ManageMoviesView>();
        manageView.SetUser(_user, l10n("admin.movies.editor.status.updated"));
        _appLoop.Display(manageView);
    }

    private void OnDeleteConfirmed()
    {
        _movieAccess.Delete(_movie.Id);

        Console.Clear();
        if (_onBack != null) { _onBack(); return; }
        var manageView = _serviceProvider.GetRequiredService<ManageMoviesView>();
        manageView.SetUser(_user, l10n("admin.movies.editor.status.deleted"));
        _appLoop.Display(manageView);
    }

    private void NavigateBack()
    {
        Console.Clear();
        if (_onBack != null)
        {
            _onBack();
            return;
        }
        var manageView = _serviceProvider.GetRequiredService<ManageMoviesView>();
        manageView.SetUser(_user);
        _appLoop.Display(manageView);
    }
}
