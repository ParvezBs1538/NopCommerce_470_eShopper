using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.AamarPay.Components;

public class AamarPayPaymentViewComponent : NopStationViewComponent
{
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ICurrencyService _currencyService;
    private readonly IStoreContext _storeContext;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;

    public AamarPayPaymentViewComponent(IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        ICurrencyService currencyService,
        IStoreContext storeContext,
        IOrderTotalCalculationService orderTotalCalculationService)
    {
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _currencyService = currencyService;
        _storeContext = storeContext;
        _orderTotalCalculationService = orderTotalCalculationService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            throw new NopException("Cart is empty");

        //customer currency
        var currentCustomerCurrencyId = currentCustomer.CurrencyId;
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

        if (customerCurrency == null || !(customerCurrency.CurrencyCode == "BDT" || customerCurrency.CurrencyCode == "USD"))
            throw new NopException("Currency is not available");

        var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        if (!shoppingCartTotal.shoppingCartTotal.HasValue)
            return Content("");

        return View("~/Plugins/NopStation.Plugin.Payments.AamarPay/Views/PaymentInfo.cshtml");
    }
}