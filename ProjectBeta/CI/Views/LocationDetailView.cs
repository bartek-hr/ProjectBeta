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
        Label(l10n("location.detail.city", new Dictionary<string, string> { ["city"] = _location.City }));
        Label(l10n("location.detail.address", new Dictionary<string, string> { ["address"] = _location.Address }));
        Label(l10n("location.detail.capacity", new Dictionary<string, string> { ["capacity"] = _location.ComputedCapacity.ToString() }));

        Divider();

        Button(l10n("location.detail.actions.movies")).OnClick(() =>
        {
            Console.Clear();
            var moviesView = _serviceProvider.GetRequiredService<MoviesView>();
            moviesView.SetUser(_user, _location.Id);
            _appLoop.Display(moviesView);
        });

        if (_user.IsSuperAdmin())
        {
            Button(l10n("location.detail.actions.edit_location")).OnClick(() =>
            {
                Console.Clear();
                var editView = _serviceProvider.GetRequiredService<LocationEditView>();
                editView.SetView(_user, _location);
                _appLoop.Display(editView);
            });
        }

        if (_user.IsAdmin())
        {
            Button(l10n("location.detail.actions.opening_times")).OnClick(() =>
            {
                Console.Clear();
                var openingTimesView = _serviceProvider.GetRequiredService<LocationOpeningTimesView>();
                openingTimesView.SetContext(_user, _location);
                _appLoop.Display(openingTimesView);
            });
        }

        Button(l10n("location.detail.actions.snack_manager")).OnClick(() =>
        {
            Console.Clear();
            var snacksView = _serviceProvider.GetRequiredService<SnacksView>();
            snacksView.SetView(_user, _location.Id);
            _appLoop.Display(snacksView);
        });

        Button(l10n("location.detail.actions.reports")).OnClick(() =>
        {
            Console.Clear();
            var reportsView = _serviceProvider.GetRequiredService<ReportsView>();
            reportsView.SetView(_user, _location);
            _appLoop.Display(reportsView);
        });

        Divider();
        Message(() => _statusMessage);
        Button(l10n("location.detail.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var locationView = _serviceProvider.GetRequiredService<LocationView>();
            locationView.SetUser(_user);
            _appLoop.Display(locationView);
        });
    }
}
