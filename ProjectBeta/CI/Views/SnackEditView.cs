using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.CI;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class SnackEditView : Form
{
    private readonly SnackLogic _snackLogic;
    private readonly AppLoop _appLoop;
    private readonly IServiceProvider _serviceProvider;
    private User _user;
    private Snack _snack;
    private string? _statusMessage;
    private bool _confirmingDelete;
    private Button? _noCancelButton;
    private Dictionary<string, string[]>? _fieldErrors;

    public SnackEditView(SnackLogic snackLogic, AppLoop appLoop, IServiceProvider serviceProvider)
    {
        _snackLogic = snackLogic;
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
    }

    public void SetView(User user, Snack snack)
    {
        _user = user;
        _snack = snack;
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("snacks.edit.heading"));
        Label(l10n("snacks.edit.instructions"));
        var table = new Table(
            l10n("snacks.edit.table.id"),
            l10n("snacks.edit.table.cinemaid"),
            l10n("snacks.edit.table.name"),
            l10n("snacks.edit.table.quantity"),
            l10n("snacks.edit.table.price")
        )
        .EmptyMessage(l10n("snacks.edit.empty"));

        table.AddRow(
            _snack.Id,
            _snack.CinemaId,
            _snack.Name,
            _snack.Quantity,
            _snack.Price
        );
        

        Add(table);
        Divider();
        Message(() => _statusMessage);
        Button(l10n("snacks.edit.actions.delete")).OnClick(OnDelete);
        Button(l10n("snacks.edit.actions.pay")).OnClick(OnRestock);

        Button(l10n("snacks.edit.actions.back")).OnClick(NavigateToMain).Hidden(() => _confirmingDelete);
    }

    private void OnDelete(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

        _snackLogic.Delete(_snack.Id, _user);
        NavigateToMain();

    }

    private void OnRestock(Form form)
    {
        _fieldErrors = null;
        _statusMessage = null;

       // _snackLogic.MarkAsPaid(_snack.Id);
        NavigateToMain();
    }

    private void NavigateToMain()
    {
        Console.Clear();
        var mainView = _serviceProvider.GetRequiredService<MainView>();
        mainView.SetUser(_user);
        _appLoop.Display(mainView);
    }

}
