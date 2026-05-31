using Microsoft.Extensions.DependencyInjection;
using ProjectBeta.Access;
using ProjectBeta.CI.Components;
using ProjectBeta.Model;

namespace ProjectBeta.CI.Views;

public sealed class SeatPriceView : Form
{
    private readonly SeatPriceAccess _seatPriceAccess;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user = null!;
    private string? _statusMessage;

    public SeatPriceView(SeatPriceAccess seatPriceAccess, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _seatPriceAccess = seatPriceAccess;
        _serviceProvider = serviceProvider;
        _appLoop = appLoop;
    }

    public void SetUser(User user)
    {
        _user = user;
        ClearChildren();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Heading(l10n("admin.seat_prices.heading"));
        Label(l10n("admin.seat_prices.instructions"));
        Divider();

        var seatPrices = _seatPriceAccess.GetAll();
        foreach (var sp in seatPrices)
        {
            var seatPrice = sp;
            var priceInput = NumberInput(
                l10n("admin.seat_prices.price_label", new Dictionary<string, string>
                {
                    ["name"] = seatPrice.Name,
                    ["price"] = seatPrice.Price.ToString("F2")
                }))
                .Min(0).Step(0.01).Precision(2).Default((double)seatPrice.Price);

            Button(l10n("admin.seat_prices.actions.save", new Dictionary<string, string>
            {
                ["name"] = seatPrice.Name
            })).OnClick(form =>
            {
                if (priceInput.Value.HasValue && priceInput.Value.Value >= 0)
                {
                    var newPrice = (decimal)priceInput.Value.Value;
                    _seatPriceAccess.UpdatePrice(seatPrice.Id, newPrice);
                    _statusMessage = l10n("admin.seat_prices.status.saved", new Dictionary<string, string>
                    {
                        ["name"] = seatPrice.Name,
                        ["price"] = newPrice.ToString("F2")
                    });
                    SetUser(_user); // refresh the form
                }
                else
                {
                    _statusMessage = l10n("admin.seat_prices.status.invalid_price");
                }
            });
            Spacer();
        }

        Divider();
        Message(() => _statusMessage);
        Button(l10n("admin.seat_prices.actions.back")).OnClick(() =>
        {
            Console.Clear();
            var mainView = _serviceProvider.GetRequiredService<MainView>();
            mainView.SetUser(_user);
            _appLoop.Display(mainView);
        });
    }
}
