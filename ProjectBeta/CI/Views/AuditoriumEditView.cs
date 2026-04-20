using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class AuditoriumEditView : Form
{
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Cinema _cinema = null!;
    private Auditorium _auditorium = null!;
    private string? _statusMessage;

    public AuditoriumEditView(AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _auditoriumLogic = auditoriumLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetContext(User user, Cinema cinema, Auditorium auditorium)
    {
        _user = user;
        _cinema = cinema;
        _auditorium = auditorium;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Message(() => $"Edit Auditorium: {_auditorium.Name}");
        Label($"Cinema: {_cinema.Name}");
        Divider();

        Message(() => _statusMessage);
        TextInput("Name").Required().Max(100).Default(_auditorium.Name);

        Divider();

        Button("Save").OnClick(form =>
        {
            var name = form.Get<string>("Name") ?? string.Empty;

            try
            {
                _auditoriumLogic.UpdateName(_auditorium.Id, name, _user);
                _auditorium.Name = name;
                _statusMessage = "Auditorium updated successfully.";
            }
            catch (UnauthorizedAccessException ex)
            {
                _statusMessage = ex.Message;
            }
        });

        Button("Back").OnClick(() =>
        {
            Console.Clear();
            var listView = _serviceProvider.GetRequiredService<AuditoriumListView>();
            listView.SetContext(_user, _cinema);
            _appLoop.Display(listView);
        });
    }
}
