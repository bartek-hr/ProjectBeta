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
        Heading("Locations");

        var searchInput = TextInput("Search by name");
        Navigation(
            Button("Search").OnClick(() =>
            {
                _searchQuery = searchInput.Value ?? string.Empty;
                RefreshView();
            }),
            Button("Clear").OnClick(() =>
            {
                _searchQuery = string.Empty;
                RefreshView();
            }));

        Divider();

        var locations = _locationLogic.Search(_searchQuery);

        var table = new Table<Location>("ID", "Name", "City", "Address", "Capacity")
            .EmptyMessage("No locations found.")
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
            bottomButtons.Add(Button("Add Location").OnClick(() =>
            {
                Console.Clear();
                var editView = _serviceProvider.GetRequiredService<LocationEditView>();
                editView.SetView(_user);
                _appLoop.Display(editView);
            }));
        }

        bottomButtons.Add(Button("Back").OnClick(() =>
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
        if (_user.IsAdmin())
        {
            Console.Clear();
            var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
            detailView.SetView(_user, location);
            _appLoop.Display(detailView);
        }
    }
}
