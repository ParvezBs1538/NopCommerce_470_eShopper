using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Extensions;
using NopStation.Plugin.Payments.Affirm.Models;

namespace NopStation.Plugin.Payments.Affirm.Components
{
    public class AffirmPaymentMethodMessageViewComponent : NopStationViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly AffirmPaymentSettings _affirmPaymentSettings;

        private readonly string _sandboxJsURL = "https://cdn1-sandbox.affirm.com/js/v2/affirm.js";
        private readonly string _canadaSandboxJsURL = "https://cdn1-sandbox.affirm.ca/js/v2/affirm.js";
        private readonly string _jsURL = "https://cdn1.affirm.com/js/v2/affirm.js";
        private readonly string _canadaJsURL = "https://cdn1.affirm.ca/js/v2/affirm.js";

        public AffirmPaymentMethodMessageViewComponent(
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            AffirmPaymentSettings affirmPaymentSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _affirmPaymentSettings = affirmPaymentSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!AffirmExtension.PluginActive())
                return Content("");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                throw new NopException("Cart is empty");

            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            if (!shoppingCartTotal.shoppingCartTotal.HasValue)
                return Content("");

            //shipping total
            var orderShippingTotalInclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true);
            var orderShippingTotalExclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, false);
            if (!orderShippingTotalInclTax.shippingTotal.HasValue || !orderShippingTotalExclTax.shippingTotal.HasValue)
                throw new NopException("Shipping total couldn't be calculated");

            var jsUrl = string.Empty;
            if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                jsUrl = _affirmPaymentSettings.UseSandbox ? _sandboxJsURL : _jsURL;
            else
                jsUrl = _affirmPaymentSettings.UseSandbox ? _canadaSandboxJsURL : _canadaJsURL;

            var model = new PromotionalPaymentInfoModel()
            {
                PublicApiKey = _affirmPaymentSettings.PublicApiKey,
                Amount = decimal.ToInt32(shoppingCartTotal.shoppingCartTotal.Value * 100),
                JsURL = jsUrl,
                SystemName = AffirmPaymentDefaults.SystemName
            };
            return View("~/Plugins/NopStation.Plugin.Payments.Affirm/Views/PaymentMethodMessage.cshtml", model);
        }
    }
}
