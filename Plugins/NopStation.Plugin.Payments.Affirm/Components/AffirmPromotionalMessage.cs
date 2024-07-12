using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Extensions;
using NopStation.Plugin.Payments.Affirm.Models;

namespace NopStation.Plugin.Payments.Affirm.Components
{
    public class AffirmPromotionalMessageViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly AffirmPaymentSettings _affirmPaymentSettings;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Constants

        private readonly string _sandboxJsURL = "https://cdn1-sandbox.affirm.com/js/v2/affirm.js";
        private readonly string _canadaSandboxJsURL = "https://cdn1-sandbox.affirm.ca/js/v2/affirm.js";
        private readonly string _jsURL = "https://cdn1.affirm.com/js/v2/affirm.js";
        private readonly string _canadaJsURL = "https://cdn1.affirm.ca/js/v2/affirm.js";

        #endregion

        #region Ctor

        public AffirmPromotionalMessageViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            AffirmPaymentSettings affirmPaymentSettings,
            ICurrencyService currencyService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _affirmPaymentSettings = affirmPaymentSettings;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!AffirmExtension.PluginActive())
                return Content("");

            string jsUrl;
            if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                jsUrl = _affirmPaymentSettings.UseSandbox ? _sandboxJsURL : _jsURL;
            else
                jsUrl = _affirmPaymentSettings.UseSandbox ? _canadaSandboxJsURL : _canadaJsURL;

            if (widgetZone == PublicWidgetZones.ProductDetailsOverviewBottom)
            {
                if (additionalData.GetType() != typeof(ProductOverviewModel) &&
                additionalData.GetType() != typeof(ProductDetailsModel))
                    return Content("");

                var model = new PromotionalPaymentInfoModel()
                {
                    PublicApiKey = _affirmPaymentSettings.PublicApiKey,
                    JsURL = jsUrl
                };

                var productOverviewModel = additionalData as ProductOverviewModel;
                if (productOverviewModel != null)
                    model.Amount = decimal.ToInt32(productOverviewModel.ProductPrice.PriceValue.Value * 100);

                var productDetailsModel = additionalData as ProductDetailsModel;
                if (productDetailsModel != null)
                    model.Amount = decimal.ToInt32(productDetailsModel.ProductPrice.PriceValue * 100);

                return View("~/Plugins/NopStation.Plugin.Payments.Affirm/Views/PromotionalPaymentInfo.cshtml", model);
            }

            if (widgetZone == PublicWidgetZones.OrderSummaryContentDeals)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new NopException("Cart is empty");

                var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                if (!shoppingCartTotalBase.HasValue)
                    return Content("");

                //shipping total
                var orderShippingTotalInclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true);
                var orderShippingTotalExclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, false);
                if (!orderShippingTotalInclTax.shippingTotal.HasValue || !orderShippingTotalExclTax.shippingTotal.HasValue)
                    throw new NopException("Shipping total couldn't be calculated");

                var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
                var model = new PromotionalPaymentInfoModel()
                {
                    PublicApiKey = _affirmPaymentSettings.PublicApiKey,
                    Amount = decimal.ToInt32(shoppingCartTotal * 100),
                    JsURL = jsUrl
                };

                return View("~/Plugins/NopStation.Plugin.Payments.Affirm/Views/PromotionalPaymentInfo.cshtml", model);
            }

            return Content("");
        }

        #endregion
    }
}
