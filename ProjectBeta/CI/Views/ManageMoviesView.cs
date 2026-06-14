using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class ManageMoviesView : Form
{
    private readonly MovieAccess _movieAccess;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = new();
    private string _searchQuery = string.Empty;
    private string? _statusMessage;

    public ManageMoviesView(MovieAccess movieAccess, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _movieAccess = movieAccess;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetUser(User user, string? statusMessage = null)
    {
        _user = user;
        _statusMessage = statusMessage;
        _searchQuery = string.Empty;
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
        var movies = _movieAccess.GetAll();
        if (!string.IsNullOrWhiteSpace(_searchQuery))
            movies = movies.Where(m => m.Title.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        Heading(l10n("admin.movies.list.heading"));

        var searchInput = TextInput(l10n("admin.movies.list.table.title"));
        Navigation(
            Button(l10n("admin.movies.list.actions.search")).OnClick(() =>
            {
                _searchQuery = searchInput.Value ?? string.Empty;
                RefreshView();
            }),
            Button(l10n("admin.movies.list.actions.clear")).OnClick(() =>
            {
                _searchQuery = string.Empty;
                RefreshView();
            }));

        Divider();

        var table = new Table<Movie>(
                l10n("admin.movies.list.table.title"),
                l10n("admin.movies.list.table.genres"),
                l10n("admin.movies.list.table.rating"),
                l10n("admin.movies.list.table.runtime"))
            .EmptyMessage(l10n("admin.movies.list.empty"))
            .OnSelect(OnMovieSelected);

        foreach (var movie in movies)
        {
            var genres = string.Join(", ", movie.Genres);
            var runtime = movie.RuntimeSeconds.HasValue ? (movie.RuntimeSeconds.Value / 60).ToString() : "-";
            var rating = movie.Rating.HasValue ? movie.Rating.Value.ToString("0.0") : "-";
            table.AddRow(movie, movie.Title, genres, rating, runtime);
        }

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Navigation(
            Button(l10n("admin.movies.list.actions.add")).OnClick(() =>
            {
                Console.Clear();
                var creatorView = _serviceProvider.GetRequiredService<MovieCreatorView>();
                creatorView.SetUser(_user);
                _appLoop.Display(creatorView);
            }),
            Button(l10n("admin.movies.list.actions.back")).OnClick(() =>
            {
                Console.Clear();
                var mainView = _serviceProvider.GetRequiredService<MainView>();
                mainView.SetUser(_user);
                _appLoop.Display(mainView);
            }));
    }

    private void OnMovieSelected(Movie movie)
    {
        Console.Clear();
        var editView = _serviceProvider.GetRequiredService<MovieEditView>();
        editView.SetView(_user, movie);
        _appLoop.Display(editView);
    }
}
