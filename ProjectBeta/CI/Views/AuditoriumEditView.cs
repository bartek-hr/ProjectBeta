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
    private Location _location = null!;
    private Auditorium? _auditorium;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public AuditoriumEditView(AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _auditoriumLogic = auditoriumLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Location location, Auditorium? auditorium)
    {
        _user = user;
        _location = location;
        _auditorium = auditorium;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        bool isNew = _auditorium == null;
        Heading(isNew ? "Add Auditorium" : $"Edit Auditorium — {_auditorium!.Name}");
        Divider();

        Message(() => GetError("general"));

        var nameInput = TextInput("Name").Key("name").Required().Min(1).Max(100);
        if (!isNew) nameInput.Default(_auditorium!.Name);
        Message(() => GetError("name"));

        var capacityInput = TextInput("Capacity").Key("capacity").Required();
        if (!isNew) capacityInput.Default(_auditorium!.Capacity.ToString());
        Message(() => GetError("capacity"));

        Divider();
        Message(() => _statusMessage);

        var buttons = new List<Button>
        {
            Button("Save").OnClick(OnSave),
            Button("Back").OnClick(NavigateBack)
        };

        if (!isNew)
            buttons.Insert(1, Button("Delete").OnClick(OnDelete));

        Navigation(buttons.ToArray());
    }

    private string? GetError(string key) =>
        _fieldErrors != null && _fieldErrors.TryGetValue(key, out var errs)
            ? string.Join("\n", errs)
            : null;

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var name = form.Get<string>("name");
        var capacityRaw = form.Get<string>("capacity");

        if (!int.TryParse(capacityRaw, out int capacity) || capacity <= 0)
        {
            _fieldErrors = new() { ["capacity"] = ["Capacity must be a positive number."] };
            Invalidate();
            Render();
            return;
        }

        try
        {
            if (_auditorium == null)
            {
                _auditoriumLogic.Add(new Auditorium { Name = name!, Capacity = capacity, LocationId = _location.Id }, _user);
            }
            else
            {
                _auditoriumLogic.UpdateName(_auditorium.Id, name!, _user);
                _auditoriumLogic.UpdateCapacity(_auditorium.Id, capacity, _user);
            }

            NavigateBack();
        }
        catch (Exception ex)
        {
            _fieldErrors = new() { ["general"] = [ex.Message] };
            Invalidate();
            Render();
        }
    }

    private void OnDelete(Form form)
    {
        _auditoriumLogic.Delete(_auditorium!.Id, _user);
        NavigateBack();
    }

    private void NavigateBack()
    {
        Console.Clear();
        var auditoriumsView = _serviceProvider.GetRequiredService<AuditoriumsView>();
        auditoriumsView.SetView(_user, _location);
        _appLoop.Display(auditoriumsView);
    }
}
