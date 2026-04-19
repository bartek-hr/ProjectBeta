using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class CinemaDetailView : Form
{
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Cinema _cinema = null!;

    public CinemaDetailView(AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetContext(User user, Cinema cinema)
    {
        _user = user;
        _cinema = cinema;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(_cinema.Name);
        Label($"City: {_cinema.City}");
        Divider();

        Button("Update Cinema").OnClick(() =>
        {
            Console.Clear();
            var editView = _serviceProvider.GetRequiredService<CinemaEditView>();
            editView.SetContext(_user, _cinema);
            _appLoop.Display(editView);
        });

        Button("View Auditoriums").OnClick(() =>
        {
            Console.Clear();
            var listView = _serviceProvider.GetRequiredService<AuditoriumListView>();
            listView.SetContext(_user, _cinema);
            _appLoop.Display(listView);
        });

        Divider();
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var cinemaView = _serviceProvider.GetRequiredService<CinemaView>();
            cinemaView.SetUser(_user);
            _appLoop.Display(cinemaView);
        });
    }
}
