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

        var locations = _locationLogic.Search(_searchQuery);

        var table = new Table<Location>("ID", "Name", "Capacity", "Auditoriums")
            .EmptyMessage("No locations found.");

        foreach (var location in locations)
        {
            var l = location;
            table.AddRow(
                l,
                l.Id.ToString(),
                l.Name,
                l.Capacity.ToString(),
                l.Auditoriums.Count.ToString()
            );
        }

        Add(table);

        Divider();
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }
}
