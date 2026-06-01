using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class AddAuditoriumsView : Form
{
    private readonly AuditoriumLogic _auditoriumLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user = null!;
    private Location _location = null!;
    private string? _statusMessage;
    private Dictionary<string, string[]>? _fieldErrors;

    public AddAuditoriumsView(AuditoriumLogic auditoriumLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _auditoriumLogic = auditoriumLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Location location)
    {
        _user = user;
        _location = location;
        _statusMessage = null;
        _fieldErrors = null;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading($"Add Auditoriums — {_location.Name}");
        Divider();

        Message(() => GetError("general"));

        Label("Small  (150 seats)");
        var smallInput = TextInput("Number of small auditoriums").Key("small").Default("0");
        Message(() => GetError("small"));

        Label("Medium (300 seats)");
        var mediumInput = TextInput("Number of medium auditoriums").Key("medium").Default("0");
        Message(() => GetError("medium"));

        Label("Large  (500 seats)");
        var largeInput = TextInput("Number of large auditoriums").Key("large").Default("0");
        Message(() => GetError("large"));

        Divider();
        Message(() => _statusMessage);
        Button("Add").OnClick(OnSave);
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

        if (smallCount + mediumCount + largeCount == 0)
        {
            _fieldErrors = new Dictionary<string, string[]> { ["general"] = ["Enter at least one auditorium to add."] };
            ClearChildren();
            InitializeForm();
            return;
        }

        try
        {
            AddAuditoriums("Small", 150, smallCount);
            AddAuditoriums("Medium", 300, mediumCount);
            AddAuditoriums("Large", 500, largeCount);
            NavigateBack();
        }
        catch (Exception ex)
        {
            _fieldErrors = new Dictionary<string, string[]> { ["general"] = [ex.Message] };
            ClearChildren();
            InitializeForm();
        }
    }

    private void AddAuditoriums(string sizeName, int capacity, int count)
    {
        var existing = _auditoriumLogic.GetByLocationId(_location.Id);
        var sameSize = existing.Count(a => a.Name.StartsWith(sizeName + " "));
        for (int i = 1; i <= count; i++)
        {
            _auditoriumLogic.Add(new Auditorium
            {
                Name = $"{sizeName} {sameSize + i}",
                Capacity = capacity,
                LocationId = _location.Id
            }, _user);
        }
    }

    private void NavigateBack()
    {
        Console.Clear();
        var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
        detailView.SetView(_user, _location);
        _appLoop.Display(detailView);
    }
}
