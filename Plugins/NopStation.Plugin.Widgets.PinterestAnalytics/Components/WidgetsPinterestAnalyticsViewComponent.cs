using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.PinterestAnalytics.Models;
using NopStation.Plugin.Widgets.PinterestAnalytics.Services;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Components
{
    public class WidgetsPinterestAnalyticsViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly PinterestAnalyticsSettings _pinterestAnalyticsSettings;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPinterestAnalyticsService _pinterestAnalyticsService;

        #endregion

        #region Ctor

        public WidgetsPinterestAnalyticsViewComponent(PinterestAnalyticsSettings pinterestAnalyticsSettings,
            ILogger logger,
            IStoreContext storeContext,
            IOrderService orderService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IProductService productService,
            ICategoryService categoryService,
            IPinterestAnalyticsService pinterestAnalyticsService,
            IWorkContext workContext)
        {
            _pinterestAnalyticsSettings = pinterestAnalyticsSettings;
            _logger = logger;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderService = orderService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _productService = productService;
            _pinterestAnalyticsService = pinterestAnalyticsService;
            _categoryService = categoryService;
        }

        #endregion

        #region Utilities

        #region Fix Illegal JavaScript Chars

        private string FixIllegalJavaScriptChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            //replace ' with \' (http://stackoverflow.com/questions/4292761/need-to-url-encode-labels-when-tracking-events-with-google-analytics)
            text = text.Replace("'", "\\'");
            return text;
        }

        #endregion

        #region Checkout Script
        public async Task<string> GetCheckoutScriptAsync(Order order)
        {
            var usCulture = new CultureInfo("en-US");

            var checkoutScript = @"pintrk('track', 'checkout', 
                       {
                         order_id:'{ORDER_ID}',
                         property: '{CUSTOMER_EMAIL}',
                         lead_type: 'Checkout page',  
                         value: {TOTAL_PRICE},
                         order_quantity: {QUANTITY},
                         currency: '{CURRENCY}',
                         content_ids:[{CONTENT_IDS}],
                         line_items: [{ITEMS}]
                       }
                    );
                    </script>";

            var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            var items = await _orderService.GetOrderItemsAsync(order.Id);
            var customer = await _workContext.GetCurrentCustomerAsync();

            checkoutScript = checkoutScript.Replace("{ORDER_ID}", FixIllegalJavaScriptChars(order.Id.ToString()));
            checkoutScript = checkoutScript.Replace("{TOTAL_PRICE}", FixIllegalJavaScriptChars(order.OrderTotal.ToString("0.00", usCulture)));
            checkoutScript = checkoutScript.Replace("{CURRENCY}", FixIllegalJavaScriptChars(currencyCode));
            checkoutScript = checkoutScript.Replace("{QUANTITY}", FixIllegalJavaScriptChars(items.Count.ToString()));
            checkoutScript = checkoutScript.Replace("{CUSTOMER_EMAIL}", FixIllegalJavaScriptChars(customer.Email));

            var sb = new StringBuilder();
            var ids = new StringBuilder();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.AppendLine(",");

                var checkoutItensScript = @"{
                      product_category:'{CATEGORY}',
                      product_name: '{PRODUCTNAME}',
                      product_id: '{PRODUCTSKU}',
                      product_price: {UNITPRICE},
                      product_quantity: {QUANTITY}
                    }
                    ";

                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var category = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).FirstOrDefault();
                var categoryName = (await _categoryService.GetCategoryByIdAsync(category.CategoryId)).Name;
                var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                var unitPrice = item.UnitPriceInclTax;
                if (string.IsNullOrEmpty(sku))
                    sku = product.Id.ToString();

                checkoutItensScript = checkoutItensScript.Replace("{PRODUCTSKU}", FixIllegalJavaScriptChars(sku));
                checkoutItensScript = checkoutItensScript.Replace("{CATEGORY}", FixIllegalJavaScriptChars(categoryName));
                checkoutItensScript = checkoutItensScript.Replace("{PRODUCTNAME}", FixIllegalJavaScriptChars(product.Name));
                checkoutItensScript = checkoutItensScript.Replace("{QUANTITY}", FixIllegalJavaScriptChars(item.Quantity.ToString()));
                checkoutItensScript = checkoutItensScript.Replace("{UNITPRICE}", FixIllegalJavaScriptChars(unitPrice.ToString("0.00", usCulture)));

                sb.AppendLine(checkoutItensScript);
                if (!string.IsNullOrEmpty(ids.ToString()))
                    ids.AppendLine(",");
                ids.AppendLine("'" + FixIllegalJavaScriptChars(sku) + "'");
            }
            checkoutScript = checkoutScript.Replace("{ITEMS}", sb.ToString());
            checkoutScript = checkoutScript.Replace("{CONTENT_IDS}", ids.ToString());
            return checkoutScript;
        }

        #endregion

        #region SHA256 Hash Generator

        public static string HashWithSHA256(string value)
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray);
        }

        #endregion

        #region Last Order

        private async Task<Order> GetLastOrderAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var order = (await _orderService.SearchOrdersAsync(storeId: store.Id, customerId: customer.Id, pageSize: 1)).FirstOrDefault();

            return order;
        }

        #endregion

        #region Ecommerce Script

        private async Task<string> GetEcommerceScriptAsync()
        {
            var analyticsTrackingScript = _pinterestAnalyticsSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("PINTERESTID", _pinterestAnalyticsSettings.PinterestId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerEmail = customer.Email;
            analyticsTrackingScript = analyticsTrackingScript.Replace("<USER_EMAIL_ADDRESS>", FixIllegalJavaScriptChars(customerEmail));
            var hashedEmail = HashWithSHA256(customerEmail);
            analyticsTrackingScript = analyticsTrackingScript.Replace("<HASHED_EMAIL_ADDRESS>", FixIllegalJavaScriptChars(hashedEmail));

            return analyticsTrackingScript;
        }

        #endregion

        #region Sign Up Script

        public string GetSignupScript(string customerEmail)
        {
            var registrationScript = @"pintrk('track', 'signup', {
                value: '{CUSTOMER_EMAIL}'
             });";

            registrationScript = registrationScript.Replace("{CUSTOMER_EMAIL}", FixIllegalJavaScriptChars(customerEmail));
            return registrationScript;
        }

        #endregion

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customScript = await _pinterestAnalyticsService.PrepareEventScriptsAsync(widgetZone);
            var model = new PublicInfoModel();

            if (widgetZone == PublicWidgetZones.HeadHtmlTag)
            {
                var script = "";
                var routeData = Url.ActionContext.RouteData;

                try
                {
                    var controller = routeData.Values["controller"];
                    var action = routeData.Values["action"];

                    if (controller == null || action == null)
                        return Content("");
                    //Special case, if we are in last step of checkout, we can use order total for conversion value
                    var isOrderCompletedPage = controller.ToString().Equals("checkout", StringComparison.InvariantCultureIgnoreCase) &&
                        action.ToString().Equals("completed", StringComparison.InvariantCultureIgnoreCase);

                    var isSignupCompletedPage = controller.ToString().Equals("customer", StringComparison.InvariantCultureIgnoreCase) &&
                       action.ToString().Equals("RegisterResult", StringComparison.InvariantCultureIgnoreCase);

                    var eventScript = "";
                    if (isOrderCompletedPage)
                    {
                        var order = await GetLastOrderAsync();
                        eventScript += await GetCheckoutScriptAsync(order);
                    }
                    if (isSignupCompletedPage)
                    {
                        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                        eventScript += GetSignupScript(currentCustomer.Email);
                    }
                    script += await GetEcommerceScriptAsync();
                    eventScript += customScript;
                    script = script.Replace("{EVENT_SCRIPT}", eventScript);
                    model.Script = script;
                }
                catch (Exception ex)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, "Error creating scripts for Pinterest eCommerce tracking", ex.ToString());
                }
                return View(model);
            }
            else
            {
                var script = @"<script>
                            {CUSTOM_SCRIPT}
                            </script>
                            ";
                script = script.Replace("CUSTOM_SCRIPT", customScript);
                model.Script = script;
                return View(model);
            }
        }

        #endregion
    }
}