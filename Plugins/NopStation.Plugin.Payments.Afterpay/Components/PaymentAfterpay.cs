using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Afterpay.Models;

namespace NopStation.Plugin.Payments.Afterpay.Components
{
    public class PaymentAfterpayViewComponent : NopStationViewComponent
    {
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly TaxSettings _taxSettings;
        private readonly ICurrencyService _currencyService;

        public PaymentAfterpayViewComponent(IPaymentPluginManager paymentPluginManager,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            TaxSettings taxSettings,
            ICurrencyService currencyService)
        {
            _paymentPluginManager = paymentPluginManager;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _taxSettings = taxSettings;
            _currencyService = currencyService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (!await _paymentPluginManager.IsPluginActiveAsync(AfterpayPaymentDefaults.PLUGIN_SYSTEM_NAME, customer, store?.Id ?? 0))
                return Content(string.Empty);

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);

            var model = new PaymentAfterpayModel
            {
                PaymentAmount = shoppingCartTotalBase.HasValue ? shoppingCartTotalBase.Value.ToString() : "0.00",
                Currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode
            };

            return View(model);
        }
    }
}