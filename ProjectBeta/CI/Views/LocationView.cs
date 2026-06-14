using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationView : Form
{
    private readonly LocationLogic _locationLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private string _searchQuery = string.Empty;
    private string? _statusMessage;

    public LocationView(LocationLogic locationLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _locationLogic = locationLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetUser(User user)
    {
        _user = user;
        _searchQuery = string.Empty;
        _statusMessage = null;
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
        Heading(l10n("location.list.heading"));

        var searchInput = TextInput(l10n("location.list.search_placeholder"));
        Navigation(
            Button(l10n("location.list.actions.search")).OnClick(() =>
            {
                _searchQuery = searchInput.Value ?? string.Empty;
                RefreshView();
            }),
            Button(l10n("location.list.actions.clear")).OnClick(() =>
            {
                _searchQuery = string.Empty;
                RefreshView();
            }));

        Divider();

        var locations = _locationLogic.Search(_searchQuery);

        var table = new Table<Location>(
                l10n("location.list.table.id"),
                l10n("location.list.table.name"),
                l10n("location.list.table.city"),
                l10n("location.list.table.address"),
                l10n("location.list.table.capacity"))
            .EmptyMessage(l10n("location.list.empty"))
            .OnSelect(OnLocationSelected);

        foreach (var location in locations)
        {
            var l = location;
            table.AddRow(
                l,
                l.Id.ToString(),
                l.Name,
                l.City,
                l.Address,
                l.ComputedCapacity.ToString()
            );
        }

        Add(table);

        Divider();
        Message(() => _statusMessage);

        var bottomButtons = new List<Button>();

        if (_user.IsSuperAdmin())
        {
            bottomButtons.Add(Button(l10n("location.list.actions.add")).OnClick(() =>
            {
                Console.Clear();
                var editView = _serviceProvider.GetRequiredService<LocationEditView>();
                editView.SetView(_user);
                _appLoop.Display(editView);
            }));
        }

        bottomButtons.Add(Button(l10n("location.list.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        }));

        Navigation(bottomButtons.ToArray());
    }

    private void OnLocationSelected(Location location)
    {
        Console.Clear();
        if (_user.IsAdmin())
        {
            var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
            detailView.SetView(_user, location);
            _appLoop.Display(detailView);
        }
        else
        {
            var moviesView = _serviceProvider.GetRequiredService<MoviesView>();
            moviesView.SetUser(_user, location.Id);
            _appLoop.Display(moviesView);
        }
    }
}
