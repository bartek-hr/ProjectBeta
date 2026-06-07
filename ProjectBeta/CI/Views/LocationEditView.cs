using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class LocationEditView : Form
{
    private readonly LocationLogic _locationLogic;
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location? _existing;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public LocationEditView(LocationLogic locationLogic, AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _locationLogic = locationLogic;
        _auditoriumLogic = auditoriumLogic;
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

        int currentSmall = 0, currentMedium = 0, currentLarge = 0;
        if (_existing != null)
        {
            var existingAuditoriums = _auditoriumLogic.GetByLocationId(_existing.Id);
            currentSmall  = existingAuditoriums.Count(a => a.Name.StartsWith("Small "));
            currentMedium = existingAuditoriums.Count(a => a.Name.StartsWith("Medium "));
            currentLarge  = existingAuditoriums.Count(a => a.Name.StartsWith("Large "));
        }

        Heading(_existing == null ? "Auditoriums to add" : "Number of auditoriums");
        Label("Small  (150 seats)");
        var smallInput = TextInput("Number of small auditoriums").Key("small").Default(currentSmall.ToString());
        Message(() => GetError("small"));

        Label("Medium (300 seats)");
        var mediumInput = TextInput("Number of medium auditoriums").Key("medium").Default(currentMedium.ToString());
        Message(() => GetError("medium"));

        Label("Large  (500 seats)");
        var largeInput = TextInput("Number of large auditoriums").Key("large").Default(currentLarge.ToString());
        Message(() => GetError("large"));

        Divider();
        Message(() => _statusMessage);
        Navigation(
            Button("Save").OnClick(OnSave),
            Button("Cancel").OnClick(NavigateBack)
        );
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
        var smallRaw = form.Get<string>("small") ?? "0";
        var mediumRaw = form.Get<string>("medium") ?? "0";
        var largeRaw = form.Get<string>("large") ?? "0";

        var errors = new Dictionary<string, string[]>();

        if (!int.TryParse(smallRaw, out var smallCount) || smallCount < 0)
            errors["small"] = ["Must be a whole number (0 or more)."];
        if (!int.TryParse(mediumRaw, out var mediumCount) || mediumCount < 0)
            errors["medium"] = ["Must be a whole number (0 or more)."];
        if (!int.TryParse(largeRaw, out var largeCount) || largeCount < 0)
            errors["large"] = ["Must be a whole number (0 or more)."];

        if (errors.Count > 0)
        {
            _fieldErrors = errors;
            ClearChildren();
            InitializeForm();
            return;
        }

        try
        {
            int locationId;
            if (_existing == null)
            {
                var location = new Location { Name = name, City = city, Address = address };
                _locationLogic.Add(location, _user);
                locationId = location.Id;
            }
            else
            {
                locationId = _existing.Id;
                _locationLogic.UpdateName(locationId, name, _user);
                _locationLogic.UpdateCity(locationId, city, _user);
                _locationLogic.UpdateAddress(locationId, address, _user);
            }

            var existingAuditoriums = _auditoriumLogic.GetByLocationId(locationId);
            int existingSmall  = existingAuditoriums.Count(a => a.Name.StartsWith("Small "));
            int existingMedium = existingAuditoriums.Count(a => a.Name.StartsWith("Medium "));
            int existingLarge  = existingAuditoriums.Count(a => a.Name.StartsWith("Large "));

            AddAuditoriums(locationId, "Small",  150, Math.Max(0, smallCount  - existingSmall));
            AddAuditoriums(locationId, "Medium", 300, Math.Max(0, mediumCount - existingMedium));
            AddAuditoriums(locationId, "Large",  500, Math.Max(0, largeCount  - existingLarge));

            NavigateBack();
        }
        catch (Exception ex)
        {
            _fieldErrors = new Dictionary<string, string[]> { ["general"] = [ex.Message] };
            ClearChildren();
            InitializeForm();
        }
    }

    private void AddAuditoriums(int locationId, string sizeName, int capacity, int count)
    {
        var existing = _auditoriumLogic.GetByLocationId(locationId);
        var sameSize = existing.Count(a => a.Name.StartsWith(sizeName + " "));
        for (int i = 1; i <= count; i++)
        {
            _auditoriumLogic.Add(new Auditorium
            {
                Name = $"{sizeName} {sameSize + i}",
                Capacity = capacity,
                LocationId = locationId
            }, _user);
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
