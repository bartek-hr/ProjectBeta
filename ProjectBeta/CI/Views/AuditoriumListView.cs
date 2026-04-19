using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class AuditoriumListView : Form
{
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Cinema _cinema = null!;

    public AuditoriumListView(AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _auditoriumLogic = auditoriumLogic;
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
        Heading($"{_cinema.Name} — Auditoriums");
        Label("Select an auditorium to edit.");
        Divider();

        var auditoriums = _auditoriumLogic.GetByCinemaId(_cinema.Id);
        foreach (var auditorium in auditoriums)
        {
            var a = auditorium;
            Button($"{a.Name} (Capacity: {a.Capacity})").OnClick(() =>
            {
                Console.Clear();
                var editView = _serviceProvider.GetRequiredService<AuditoriumEditView>();
                editView.SetContext(_user, _cinema, a);
                _appLoop.Display(editView);
            });
        }

        Divider();
        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
            detailView.SetContext(_user, _cinema);
            _appLoop.Display(detailView);
        });
    }
}
