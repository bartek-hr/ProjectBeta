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
        Message(() => $"Edit Cinema: {_cinema.Name}");
        Divider();

        Message(() => _statusMessage);
        TextInput("Name").Required().Max(100).Default(_cinema.Name);
        TextInput("City").Required().Max(100).Default(_cinema.City);

        Divider();

        Button("Save").OnClick(form =>
        {
            var name = form.Get<string>("Name") ?? string.Empty;
            var city = form.Get<string>("City") ?? string.Empty;

            try
            {
                _cinemaLogic.UpdateName(_cinema.Id, name, _user);
                _cinemaLogic.UpdateCity(_cinema.Id, city, _user);
                _cinema.Name = name;
                _cinema.City = city;
                _statusMessage = "Cinema updated successfully.";
            }
            catch (UnauthorizedAccessException ex)
            {
                _statusMessage = ex.Message;
            }
        });

        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var updated = _cinemaLogic.GetById(_cinema.Id) ?? _cinema;
            var detailView = _serviceProvider.GetRequiredService<CinemaDetailView>();
            detailView.SetContext(_user, updated);
            _appLoop.Display(detailView);
        });
    }
}
