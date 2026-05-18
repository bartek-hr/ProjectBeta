using System.Reflection.Metadata;
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
    private int _cinemaId;
    private Booking _booking;
    private Dictionary<string, int> _chosenSnacksCount = new();
    private List<Snack> _chosenSnacks = new();
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

    public void SetView(User user, Booking createdBooking, int cinemaId)
    {
        _user = user;
        _cinemaId = cinemaId;
        _booking = createdBooking;
        _searchQuery = string.Empty;
        InitializeForm();
    }

    private void InitializeForm()
    {
        ClearChildren();
        List<Snack> Snacks = _snackLogic.Search(_cinemaId, _searchQuery);
        Heading(l10n("Current Snacks"));

        var searchInput = TextInput("Search by name");
        Button("Search").OnClick(() =>
        {
            _searchQuery = searchInput.Value ?? string.Empty;
            InitializeForm();
            _appLoop.Display(this);
        });
        Button("Clear").OnClick(() =>
        {
            _searchQuery = string.Empty;
            InitializeForm();
            _appLoop.Display(this);
        });

        Divider();
        var table = new Table<Snack>(
            l10n("Name"),
            l10n("Price"),
            l10n("Selected")
        )
        .EmptyMessage(l10n("empty"))
        .OnSelect(OnSelectedSnack);

        foreach (var snack in Snacks)
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
        Message(() => _statusMessage);
        Button(l10n("Done")).OnClick(SaveBookingSnacks).Hidden(() => _confirmingDelete);
        Button(l10n("Back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
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
        foreach (var chosenSnack in _chosenSnacks)
        {
            BookingSnack SnackToAdd = new BookingSnack
            {
                SnackId = chosenSnack.Id,
                BookingId = _booking.Id,
                BookedQuantity = _chosenSnacksCount[chosenSnack.Name]
            };
            _bookingSnackLogic.Add(SnackToAdd, _user);
        }
        NavigateToMain();
    }

    private void OnSelectedSnack(Snack snack)
    {
        if (!_chosenSnacksCount.ContainsKey(snack.Name))
        {
            _chosenSnacks.Add(snack);
        }
        if (_chosenSnacksCount.ContainsKey(snack.Name))
        {
            _chosenSnacksCount[snack.Name]++;
        }
        else
        {
            _chosenSnacksCount[snack.Name] = 1;
        }

        InitializeForm();
        _appLoop.Display(this);
    }
}
