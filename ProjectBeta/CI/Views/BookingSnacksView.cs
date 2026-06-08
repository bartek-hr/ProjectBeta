using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class BookingSnacksView : Form
{
    private readonly BookingSnackLogic _bookingSnackLogic;
    private readonly SnackLogic _snackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private int _locationId;
    private Booking _booking;
    private Dictionary<string, int> _chosenSnacksCount = new();
    private List<(Snack snack, NumberInput input)> _snackInputs = new();
    private string _searchQuery = string.Empty;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public BookingSnacksView(BookingSnackLogic bookingSnackLogic, SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingSnackLogic = bookingSnackLogic;
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Booking createdBooking, int locationId)
    {
        _user = user;
        _locationId = locationId;
        _booking = createdBooking;
        _searchQuery = string.Empty;
        InitializeForm();
    }

    private void InitializeForm()
    {
        CaptureSnackCounts();
        ClearChildren();
        List<Snack> snacks = _snackLogic.Search(_locationId, _searchQuery);
        Heading("Select Snacks");

        var searchInput = TextInput("Search by name");
        Navigation(
            Button("Search").OnClick(() =>
            {
                CaptureSnackCounts();
                _searchQuery = searchInput.Value ?? string.Empty;
                InitializeForm();
                _appLoop.Display(this);
            }),
            Button("Clear").OnClick(() =>
            {
                CaptureSnackCounts();
                _searchQuery = string.Empty;
                InitializeForm();
                _appLoop.Display(this);
            }));

        Divider();
        _snackInputs = new List<(Snack, NumberInput)>();

        if (snacks.Count == 0)
        {
            Label("No snacks available.");
        }
        else
        {
            foreach (var snack in snacks)
            {
                Label($"{snack.Name}  —  €{snack.Price:0.00}");
                var currentQty = _chosenSnacksCount.GetValueOrDefault(snack.Name, 0);
                var input = NumberInput($"Quantity ({snack.Name})")
                    .Min(0)
                    .Default(currentQty);
                _snackInputs.Add((snack, input));
            }
        }

        Divider();
        Message(() => _statusMessage);
        var doneButton = Button("Done").OnClick(SaveBookingSnacks);
        var backButton = Button("Back").OnClick(NavigateToMain);
        Navigation(doneButton, backButton);
    }

    private void CaptureSnackCounts()
    {
        foreach (var (snack, input) in _snackInputs)
        {
            var qty = (int)(input.Value ?? 0);
            if (qty > 0)
                _chosenSnacksCount[snack.Name] = qty;
            else
                _chosenSnacksCount.Remove(snack.Name);
        }
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }
    private void SaveBookingSnacks()
    {
        CaptureSnackCounts();
        foreach (var (snack, _) in _snackInputs)
        {
            if (!_chosenSnacksCount.TryGetValue(snack.Name, out var qty) || qty <= 0)
                continue;
            _bookingSnackLogic.Add(new BookingSnack
            {
                SnackId = snack.Id,
                BookingId = _booking.Id,
                BookedQuantity = qty
            }, _user);
        }
        NavigateToMain();
    }
}
