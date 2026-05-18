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
        Heading(l10n("admin.cinemas.list.heading"));
        Label(l10n("admin.cinemas.list.instructions"));
        Divider();

        var cinemas = _cinemaLogic.GetAll();
        var cinemaButtons = new List<Button>();
        foreach (var cinema in cinemas)
        {
            var c = cinema;
            cinemaButtons.Add(Button(l10n("admin.cinemas.list.cinema_button", new Dictionary<string, string>
            {
                ["name"] = c.Name,
                ["city"] = c.City
            })).OnClick(() =>
            {
                Console.Clear();
                var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
                detailView.SetContext(_user, c);
                _appLoop.Display(detailView);
            }));
        }
        Navigation(cinemaButtons.ToArray());

        Divider();
        Button(l10n("admin.cinemas.list.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }
}
