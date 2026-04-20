using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class CinemaView : Form
{
    private readonly CinemaLogic _cinemaLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;

    public CinemaView(CinemaLogic cinemaLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _cinemaLogic = cinemaLogic;
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
        Heading("Cinema Management");
        Label("Select a cinema to manage.");
        Divider();

        var cinemas = _cinemaLogic.GetAll();
        foreach (var cinema in cinemas)
        {
            var c = cinema;
            Button($"{c.Name} — {c.City}").OnClick(() =>
            {
                Console.Clear();
                var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
                detailView.SetContext(_user, c);
                _appLoop.Display(detailView);
            });
        }

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
