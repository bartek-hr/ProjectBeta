using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class CinemaEditView : Form
{
    private readonly CinemaLogic _cinemaLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Cinema _cinema = null!;
    private string? _statusMessage;

    public CinemaEditView(CinemaLogic cinemaLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _cinemaLogic = cinemaLogic;
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
        Message(() => l10n("admin.cinemas.edit.heading", new Dictionary<string, string> { ["name"] = _cinema.Name }));
        Divider();

        Message(() => _statusMessage);
        TextInput(l10n("admin.cinemas.edit.fields.name.label")).Key("name").Required().Max(100).Default(_cinema.Name);
        TextInput(l10n("admin.cinemas.edit.fields.city.label")).Key("city").Required().Max(100).Default(_cinema.City);

        Divider();

        Navigation(
            Button(l10n("admin.cinemas.edit.actions.save")).OnClick(form =>
            {
                var name = form.Get<string>("name") ?? string.Empty;
                var city = form.Get<string>("city") ?? string.Empty;

                try
                {
                    _cinemaLogic.UpdateName(_cinema.Id, name, _user);
                    _cinemaLogic.UpdateCity(_cinema.Id, city, _user);
                    _cinema.Name = name;
                    _cinema.City = city;
                    _statusMessage = l10n("admin.cinemas.edit.status.updated");
                }
                catch (UnauthorizedAccessException ex)
                {
                    _statusMessage = ex.Message;
                }
            }),
            Button(l10n("admin.cinemas.edit.actions.back")).OnClick(() =>
            {
                Console.Clear();
                var updated = _cinemaLogic.GetById(_cinema.Id) ?? _cinema;
                var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
                detailView.SetContext(_user, updated);
                _appLoop.Display(detailView);
            }));
    }
}
