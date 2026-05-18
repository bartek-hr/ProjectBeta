using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class SnacksView : Form
{
    private readonly SnackLogic _snackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private int _locationId;
    private string _searchQuery = string.Empty;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Dictionary<string, string[]>? _fieldErrors;

    public SnacksView(SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, int locationId)
    {
        _user = user;
        _locationId = locationId;
        _searchQuery = string.Empty;
        ClearChildren();
        InitializeForm();
    }

    private void RefreshView()
    {
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        List<Snack> snacks = _snackLogic.Search(_locationId, _searchQuery);
        Heading(l10n("Current Snacks"));

        var searchInput = TextInput("Search by name");
        Button("Search").OnClick(() =>
        {
            _searchQuery = searchInput.Value ?? string.Empty;
            RefreshView();
        });
        Button("Clear").OnClick(() =>
        {
            _searchQuery = string.Empty;
            RefreshView();
        });

        Divider();
        var table = new Table<Snack>(
            l10n("Id"),
            l10n("Name"),
            l10n("Price"),
            l10n("Quantity"),
            l10n("LocationId")
        )
        .EmptyMessage(l10n("empty"))
        .OnSelect(OnSnackSelected);

        foreach (var snack in snacks)
        {
            table.AddRow(
                snack,
                snack.Id,
                snack.Name,
                snack.Price,
                snack.Quantity,
                snack.LocationId
            );
        }

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Button(l10n("Add")).OnClick(NavigateToNewSnack).Hidden(() => _confirmingDelete);
        Button(l10n("reservations.history.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

    private void NavigateToNewSnack()
    {
        Console.Clear();
        var snackCreatorView = _serviceProvider.GetRequiredService<SnackCreatorView>();
        snackCreatorView.SetView(_user, _locationId);
        _appLoop.Display(snackCreatorView);
    }

    private void OnSnackSelected(Snack snack)
    {
        Console.Clear();
        var snackEditView = _serviceProvider.GetRequiredService<SnackEditView>();
        snackEditView.SetView(_user, snack);
        _appLoop.Display(snackEditView);
    }
}
