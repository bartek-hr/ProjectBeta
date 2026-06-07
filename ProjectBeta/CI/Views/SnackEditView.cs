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

        var table = new Table(
            "ID",
            "Location ID",
            "Name",
            "Quantity",
            "Price"
        )
        .EmptyMessage("No snack found.");

        table.AddRow(
            _snack.Id,
            _snack.LocationId,
            _snack.Name,
            _snack.Quantity,
            _snack.Price
        );
        

        Add(table);
        Divider();
        Message(() => _statusMessage);
        var backButton = Button("Back").OnClick(NavigateToMain);
        backButton.Hidden(() => _confirmingDelete);
        Navigation(
            Button("Delete").OnClick(OnDelete),
            Button("Restock").OnClick(OnRestock),
            backButton);
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
