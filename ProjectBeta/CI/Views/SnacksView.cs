using System.Reflection.Metadata;
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
    private int _cinemaId;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public SnacksView(SnackLogic SnackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = SnackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, int cinemaId)
    {
        _user = user;
        _cinemaId = cinemaId;
        InitializeForm();
    }

    private void InitializeForm()
    {
        List<Snack> Snacks = _snackLogic.GetAllByCinemaId(_cinemaId);
        Heading(l10n("Current Snacks"));
        var table = new Table<Snack>(
            l10n("Id"),
            l10n("Name"),
            l10n("Price"),
            l10n("Quantity"),
            l10n("CinemaId")
        )
        .EmptyMessage(l10n("empty"))
        .OnSelect(OnSnackSelected);

        foreach (var snack in Snacks)
        {
            
            table.AddRow(
                snack,
                snack.Id,
                snack.Name,
                snack.Price,
                snack.Quantity,
                snack.CinemaId
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
        snackCreatorView.SetView(_user);
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
