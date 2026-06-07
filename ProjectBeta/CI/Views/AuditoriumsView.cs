using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class AuditoriumsView : Form
{
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location _location = null!;
    private string? _statusMessage;

    public AuditoriumsView(AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _auditoriumLogic = auditoriumLogic;
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
        Heading($"Auditoriums — {_location.Name}");
        Divider();

        var auditoriums = _auditoriumLogic.GetByLocationId(_location.Id);

        var table = new Table<Auditorium>("Id", "Name", "Capacity")
            .EmptyMessage("No auditoriums found.")
            .OnSelect(OnAuditoriumSelected);

        foreach (var a in auditoriums)
            table.AddRow(a, a.Id, a.Name, a.Capacity);

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Navigation(
            Button("Add").OnClick(NavigateToAdd),
            Button("Back").OnClick(NavigateBack));
    }

    private void NavigateToAdd()
    {
        Console.Clear();
        var editView = _serviceProvider.GetRequiredService<AuditoriumEditView>();
        editView.SetView(_user, _location, null);
        _appLoop.Display(editView);
    }

    private void OnAuditoriumSelected(Auditorium auditorium)
    {
        Console.Clear();
        var editView = _serviceProvider.GetRequiredService<AuditoriumEditView>();
        editView.SetView(_user, _location, auditorium);
        _appLoop.Display(editView);
    }

    private void NavigateBack()
    {
        Console.Clear();
        var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
        detailView.SetView(_user, _location);
        _appLoop.Display(detailView);
    }
}
