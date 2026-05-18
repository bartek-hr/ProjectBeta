using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationEditView : Form
{
    private readonly LocationLogic _locationLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location? _existing;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public LocationEditView(LocationLogic locationLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _locationLogic = locationLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Location? existing = null)
    {
        _user = user;
        _existing = existing;
        _statusMessage = null;
        _fieldErrors = null;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(_existing == null ? "Add Location" : "Edit Location");
        Divider();

        Message(() => GetError("general"));

        var nameInput = TextInput("Name").Key("name").Required().Min(1).Max(100);
        if (!string.IsNullOrEmpty(_existing?.Name)) nameInput.Default(_existing.Name);
        Message(() => GetError("name"));

        var cityInput = TextInput("City").Key("city").Required().Min(1).Max(100);
        if (!string.IsNullOrEmpty(_existing?.City)) cityInput.Default(_existing.City);
        Message(() => GetError("city"));

        var addressInput = TextInput("Address").Key("address").Required().Min(1).Max(200);
        if (!string.IsNullOrEmpty(_existing?.Address)) addressInput.Default(_existing.Address);
        Message(() => GetError("address"));

        Divider();
        Message(() => _statusMessage);
        Button("Save").OnClick(OnSave);
        Button("Cancel").OnClick(NavigateBack);
    }

    private string? GetError(string key)
    {
        return _fieldErrors != null && _fieldErrors.ContainsKey(key)
            ? string.Join("\n", _fieldErrors[key])
            : null;
    }

    private void OnSave(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        var name = form.Get<string>("name") ?? string.Empty;
        var city = form.Get<string>("city") ?? string.Empty;
        var address = form.Get<string>("address") ?? string.Empty;

        try
        {
            if (_existing == null)
            {
                _locationLogic.Add(new Location { Name = name, City = city, Address = address }, _user);
            }
            else
            {
                _existing.Name = name;
                _existing.City = city;
                _existing.Address = address;
                _locationLogic.UpdateName(_existing.Id, name, _user);
                _locationLogic.UpdateCity(_existing.Id, city, _user);
                _locationLogic.UpdateAddress(_existing.Id, address, _user);
            }

            NavigateBack();
        }
        catch (Exception ex)
        {
            _fieldErrors = new Dictionary<string, string[]> { ["general"] = [ex.Message] };
            ClearChildren();
            InitializeForm();
        }
    }

    private void NavigateBack()
    {
        Console.Clear();
        var locationView = _serviceProvider.GetRequiredService<LocationView>();
        locationView.SetUser(_user);
        _appLoop.Display(locationView);
    }
}
