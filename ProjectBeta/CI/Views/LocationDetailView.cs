using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationDetailView : Form
{
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location _location = null!;
    private string? _statusMessage;

    public LocationDetailView(AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Location location)
    {
        _user = user;
        _location = location;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(_location.Name);
        Label($"City: {_location.City}");
        Label($"Address: {_location.Address}");
        Label($"Total capacity: {_location.ComputedCapacity}");

        Divider();

        Button("Movies").OnClick(() =>
        {
            Console.Clear();
            var moviesView = _serviceProvider.GetRequiredService<MoviesView>();
            moviesView.SetUser(_user, _location.Id);
            _appLoop.Display(moviesView);
        });

        if (_user.IsSuperAdmin())
        {
            Button("Edit Location").OnClick(() =>
            {
                Console.Clear();
                var editView = _serviceProvider.GetRequiredService<LocationEditView>();
                editView.SetView(_user, _location);
                _appLoop.Display(editView);
            });
        }

        if (_user.IsAdmin())
        {
            Button("Add Auditoriums").OnClick(() =>
            {
                Console.Clear();
                var addAuditoriumsView = _serviceProvider.GetRequiredService<AddAuditoriumsView>();
                addAuditoriumsView.SetView(_user, _location);
                _appLoop.Display(addAuditoriumsView);
            });
        }

        Button("Snack Manager").OnClick(() =>
        {
            Console.Clear();
            var snacksView = _serviceProvider.GetRequiredService<SnacksView>();
            snacksView.SetView(_user, _location.Id);
            _appLoop.Display(snacksView);
        });

        Button("Reports").OnClick(() =>
        {
            Console.Clear();
            var reportsView = _serviceProvider.GetRequiredService<ReportsView>();
            reportsView.SetView(_user, _location);
            _appLoop.Display(reportsView);
        });

        Divider();
        Message(() => _statusMessage);
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var locationView = _serviceProvider.GetRequiredService<LocationView>();
            locationView.SetUser(_user);
            _appLoop.Display(locationView);
        });
    }
}
