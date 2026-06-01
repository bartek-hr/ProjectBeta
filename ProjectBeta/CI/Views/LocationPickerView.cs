using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationPickerView : Form
{
    private readonly LocationLogic _locationLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;

    public LocationPickerView(LocationLogic locationLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _locationLogic = locationLogic;
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
        Heading("Select a Location");

        var locations = _locationLogic.GetAll();

        var table = new Table<Location>("ID", "Name", "City", "Address", "Capacity")
            .EmptyMessage("No locations available.")
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
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }

    private void OnLocationSelected(Location location)
    {
        Console.Clear();
        var moviesView = _serviceProvider.GetRequiredService<MoviesView>();
        moviesView.SetUser(_user, location.Id);
        _appLoop.Display(moviesView);
    }
}
