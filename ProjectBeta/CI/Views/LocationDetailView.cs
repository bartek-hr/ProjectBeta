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

        var actionButtons = new List<Button>
        {
            Button(l10n("location.detail.actions.movies")).OnClick(() =>
            {
                Console.Clear();
                var moviesView = _serviceProvider.GetRequiredService<MoviesView>();
                moviesView.SetUser(_user, _location.Id);
                _appLoop.Display(moviesView);
            })
        };

        if (_user.IsAdmin())
        {
            actionButtons.Add(Button(l10n("location.detail.actions.auditoriums")).OnClick(() =>
            {
                Console.Clear();
                var auditoriumsView = _serviceProvider.GetRequiredService<AuditoriumsView>();
                auditoriumsView.SetView(_user, _location);
                _appLoop.Display(auditoriumsView);
            }));

            actionButtons.Add(Button(l10n("location.detail.actions.snack_manager")).OnClick(() =>
            {
                Console.Clear();
                var snacksView = _serviceProvider.GetRequiredService<SnacksView>();
                snacksView.SetView(_user, _location.Id);
                _appLoop.Display(snacksView);
            }));

            actionButtons.Add(Button(l10n("location.detail.actions.reports")).OnClick(_ =>
            {
                _statusMessage = l10n("location.detail.status.reports_tbd");
            }));
        }

        actionButtons.Add(Button(l10n("location.detail.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var locationView = _serviceProvider.GetRequiredService<LocationView>();
            locationView.SetUser(_user);
            _appLoop.Display(locationView);
        }));

        Navigation(actionButtons.ToArray());
        Divider();
        Message(() => _statusMessage);
    }
}
