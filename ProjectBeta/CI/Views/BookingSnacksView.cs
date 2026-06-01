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
    private readonly LocationLogic _locationLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private int _locationId;
    private Booking _booking;
    private Dictionary<string, int> _chosenSnacksCount = new();
    private List<Snack> _chosenSnacks = new();
    private string _searchQuery = string.Empty;
    private string? _statusMessage;
    private Snack? _selectedSnack;
    private Dictionary<string, string[]>? _fieldErrors;

    public BookingSnacksView(BookingSnackLogic bookingSnackLogic, SnackLogic snackLogic, LocationLogic locationLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _bookingSnackLogic = bookingSnackLogic;
        _snackLogic = snackLogic;
        _locationLogic = locationLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Booking createdBooking, int locationId)
    {
        _user = user;
        _locationId = locationId;
        _booking = createdBooking;
        _searchQuery = string.Empty;
        _selectedSnack = null;
        InitializeForm();
    }

    private void InitializeForm()
    {
        ClearChildren();
        List<Snack> snacks = _snackLogic.Search(_locationId, _searchQuery);
        Heading(l10n("booking_snacks.heading"));

        var searchInput = TextInput(l10n("booking_snacks.search_placeholder"));
        Navigation(
            Button(l10n("booking_snacks.actions.search")).OnClick(() =>
            {
                _searchQuery = searchInput.Value ?? string.Empty;
                _selectedSnack = null;
                InitializeForm();
                _appLoop.Display(this);
            }),
            Button(l10n("booking_snacks.actions.clear")).OnClick(() =>
            {
                _searchQuery = string.Empty;
                _selectedSnack = null;
                InitializeForm();
                _appLoop.Display(this);
            }));

        Divider();
        var table = new Table<Snack>(
            l10n("booking_snacks.table.name"),
            l10n("booking_snacks.table.price"),
            l10n("booking_snacks.table.selected")
        )
        .EmptyMessage(l10n("booking_snacks.empty"))
        .OnSelect(OnSelectedSnack);

        foreach (var snack in snacks)
        {
            table.AddRow(
                snack,
                snack.Name,
                snack.Price,
                _chosenSnacksCount.GetValueOrDefault(snack.Name, 0)
            );
        }

        Add(table);
        Divider();

        // Quantity input appears when a snack is selected
        if (_selectedSnack != null)
        {
            var snackName = _selectedSnack.Name;
            Label(l10n("booking_snacks.quantity_label", new Dictionary<string, string> { ["name"] = snackName }));
            var quantityInput = NumberInput(l10n("booking_snacks.quantity_input_label"))
                .Min(1)
                .Default(1);

            Navigation(
                Button(l10n("booking_snacks.actions.add")).OnClick(() =>
                {
                    var qty = (int)(quantityInput.EffectiveValue ?? 1);
                    if (!_chosenSnacks.Any(s => s.Name == snackName))
                        _chosenSnacks.Add(_selectedSnack);
                    _chosenSnacksCount[snackName] = _chosenSnacksCount.GetValueOrDefault(snackName, 0) + qty;
                    _selectedSnack = null;
                    InitializeForm();
                    _appLoop.Display(this);
                }),
                Button(l10n("booking_snacks.actions.cancel_selection")).OnClick(() =>
                {
                    _selectedSnack = null;
                    InitializeForm();
                    _appLoop.Display(this);
                }));
            Divider();
        }

        Message(() => _statusMessage);
        Navigation(
            Button(l10n("booking_snacks.actions.done")).OnClick(SaveBookingSnacks),
            Button(l10n("booking_snacks.actions.back")).OnClick(NavigateToLocation));
    }

    private void NavigateToLocation()
    {
        Console.Clear();
        var location = _locationLogic.GetById(_locationId);
        if (location != null)
        {
            var detailView = _serviceProvider.GetRequiredService<LocationDetailView>();
            detailView.SetView(_user, location);
            _appLoop.Display(detailView);
        }
        else
        {
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        }
    }

    private void SaveBookingSnacks()
    {
        foreach (var chosenSnack in _chosenSnacks)
        {
            BookingSnack snackToAdd = new BookingSnack
            {
                SnackId = chosenSnack.Id,
                BookingId = _booking.Id,
                BookedQuantity = _chosenSnacksCount[chosenSnack.Name]
            };
            _bookingSnackLogic.Add(snackToAdd, _user);
        }
        NavigateToLocation();
    }

    private void OnSelectedSnack(Snack snack)
    {
        _selectedSnack = snack;
        InitializeForm();
        _appLoop.Display(this);
    }
}
