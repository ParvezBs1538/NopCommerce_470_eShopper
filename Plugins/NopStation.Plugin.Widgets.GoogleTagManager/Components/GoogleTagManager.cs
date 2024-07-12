using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.GoogleTagManager.Models;
using NopStation.Plugin.Widgets.GoogleTagManager.Services;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Components
{
    public class GoogleTagManagerViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly GoogleTagManagerSettings _googleTagManagerConfigurationSettings;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly INopStationContext _nopStationContext;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGTMService _gtmService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public GoogleTagManagerViewComponent(GoogleTagManagerSettings googleTagManagerConfigurationSettings,
            IProductService productService,
            IWorkContext workContext,
            IStoreContext storeContext,
            INopStationContext nopStationContext,
            OrderSettings orderSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            IHttpContextAccessor httpContextAccessor,
            IShoppingCartService shoppingCartService,
            IGTMService gtmService,
            IUrlRecordService urlRecordService,
            IPriceCalculationService priceCalculationService,
            ICurrencyService currencyService)
        {
            _googleTagManagerConfigurationSettings = googleTagManagerConfigurationSettings;
            _productService = productService;
            _workContext = workContext;
            _storeContext = storeContext;
            _nopStationContext = nopStationContext;
            _orderSettings = orderSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _httpContextAccessor = httpContextAccessor;
            _shoppingCartService = shoppingCartService;
            _gtmService = gtmService;
            _urlRecordService = urlRecordService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
        }

        #endregion

        #region Utility

        private string FixIllegalJavaScriptChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("'", "\\'");
            return text;
        }

        private async Task<string> GetGTMScriptByZoneAsync(string widgetZones)
        {
            var gtmScript = "";
            if (widgetZones == PublicWidgetZones.HeadHtmlTag)
            {
                gtmScript = GoogleTagManagerDefaults.HeadTrackingScript.Replace("%GTMCONTAINERID%", _googleTagManagerConfigurationSettings.GTMContainerId);

                var routeData = Url.ActionContext.RouteData;
                var controller = routeData.Values["controller"].ToString();
                var action = routeData.Values["action"].ToString();
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var currencyCode = currentCurrency.CurrencyCode;


                if (controller == null || action == null)
                {
                    gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", "");
                }
                else if (controller.Equals("Product", StringComparison.InvariantCultureIgnoreCase) && action.Equals("ProductDetails",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    var productId = Convert.ToInt32(routeData.Values["productid"].ToString());
                    var product = await _productService.GetProductByIdAsync(productId);
                    var (priceWithoutDiscount, _, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, 0, true, quantity: product.OrderMinimumQuantity);
                    var viewItemScript = GoogleTagManagerDefaults.BaseEventScript;
                    var productSku = product.Sku;

                    if (string.IsNullOrEmpty(productSku))
                        productSku = productId.ToString();
                    viewItemScript = viewItemScript.Replace("%product_ids%", "'" + FixIllegalJavaScriptChars(productSku) + "'");
                    viewItemScript = viewItemScript.Replace("%page_type%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.PRODUCT));
                    viewItemScript = viewItemScript.Replace("%event_name%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.VIEW_ITEM));
                    var price = priceWithoutDiscount * product.OrderMinimumQuantity;
                    price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(price, currentCurrency);
                    var unitPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.Price, currentCurrency);
                    viewItemScript = viewItemScript.Replace("%value%", price.ToString("0.00", CultureInfo.InvariantCulture));
                    viewItemScript = viewItemScript.Replace("%currency%", FixIllegalJavaScriptChars(currencyCode));
                    viewItemScript = viewItemScript.Replace("%productInformation%", await _gtmService.GetProductDetailsAsync(productId, product.OrderMinimumQuantity));
                    viewItemScript = viewItemScript.Replace("%unitPrice%", FixIllegalJavaScriptChars(unitPrice.ToString("0.00", CultureInfo.InvariantCulture)));

                    gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", viewItemScript);
                }
                else if ((await _nopStationContext.GetRouteNameAsync()).Equals("CheckoutOnePage", StringComparison.InvariantCultureIgnoreCase)
                    || (await _nopStationContext.GetRouteNameAsync()).Equals("CheckoutBillingAddress", StringComparison.InvariantCultureIgnoreCase))
                {
                    var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(storeScope);

                    if (!_orderSettings.CheckoutDisabled && googleTagManagerSettings.IsEnable)
                    {
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                        if (cart.Any())
                        {
                            var (_, _, cartSubTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);

                            cartSubTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(cartSubTotal, currentCurrency);
                            var checkoutScript = GoogleTagManagerDefaults.BaseEventScript;
                            var ids = new StringBuilder();
                            foreach (var cartItem in cart)
                            {
                                if (ids.Length > 0)
                                    ids.Append(",");
                                ids.AppendLine("'" + await _gtmService.GetProductIdAsync(cartItem.ProductId) + "'");
                            }

                            checkoutScript = checkoutScript.Replace("%currency%", FixIllegalJavaScriptChars(currencyCode));
                            checkoutScript = checkoutScript.Replace("%event_name%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.BEGIN_CHECKOUT));
                            checkoutScript = checkoutScript.Replace("%page_type%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.CHECKOUT_PAGE));
                            checkoutScript = checkoutScript.Replace("%value%", cartSubTotal.ToString("0.00", CultureInfo.InvariantCulture));
                            checkoutScript = checkoutScript.Replace("%product_ids%", ids.ToString());

                            var productList = await _gtmService.PrepareProductItemsAsync(cart);
                            checkoutScript = checkoutScript.Replace("%productInformation%", productList);
                            gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", checkoutScript);
                        }
                    }
                }
                else if ((await _nopStationContext.GetRouteNameAsync()).Equals("ShoppingCart", StringComparison.InvariantCultureIgnoreCase))
                {
                    var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
                    var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(storeScope);
                    if (!_orderSettings.CheckoutDisabled && googleTagManagerSettings.IsEnable)
                    {
                        var viewCartScript = await _gtmService.PrepareShoppingCartScriptAsync(customer, storeScope);
                        gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", viewCartScript);
                    }
                }
                else
                {
                    var key = string.Format(GoogleTagManagerDefaults.SessionKey, customer.CustomerGuid.ToString());
                    var script = _httpContextAccessor.HttpContext.Session.GetString(key);
                    if (!string.IsNullOrEmpty(script))
                    {
                        gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", script);
                        _httpContextAccessor.HttpContext.Session.Remove(key);

                    }
                    else
                        gtmScript = gtmScript.Replace("%TRACKINGINFORMATION%", "");
                }
            }
            else if (widgetZones == PublicWidgetZones.BodyStartHtmlTagAfter)
            {
                gtmScript = GoogleTagManagerDefaults.BodyTrackingScript;
                gtmScript = gtmScript.Replace("%GTMCONTAINERID%", _googleTagManagerConfigurationSettings.GTMContainerId);
            }
            return gtmScript;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_googleTagManagerConfigurationSettings.IsEnable)
                return Content("");

            var currentUrl = _httpContextAccessor.HttpContext.Request.GetDisplayUrl();
            var path = currentUrl.Split('/');
            var slug = path[path.Length - 1];
            var pageType = "home";
            if (!string.IsNullOrEmpty(slug))
            {
                var urlRecord = await _urlRecordService.GetBySlugAsync(slug);
                if (urlRecord != null)
                {
                    pageType = urlRecord.EntityName;
                }
            }
            var model = new PublicInfoModel()
            {
                Script = await GetGTMScriptByZoneAsync(widgetZone)
            };
            ViewData["pageType"] = pageType;
            return View("~/Plugins/NopStation.Plugin.Widgets.GoogleTagManager/Views/Shared/Components/GoogleTagManager/Default.cshtml", model);
        }

        #endregion
    }
}
