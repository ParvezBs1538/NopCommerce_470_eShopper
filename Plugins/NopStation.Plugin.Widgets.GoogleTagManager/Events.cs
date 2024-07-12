using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Widgets.GoogleTagManager.Services;

namespace NopStation.Plugin.Widgets.GoogleTagManager
{
    public class Events : IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<PageRenderingEvent>,
        IConsumer<CustomerRegisteredEvent>,
        IConsumer<ProductSearchEvent>
    {
        private const string ORDER_ALREADY_PROCESSED = "NopStation.GoogleTagManager.OrderAlreadyProcessed";

        private readonly INopHtmlHelper _nopHtmlHelper;
        private readonly IGTMService _gtm_Service;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IGTMService _gtmService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;

        public Events(INopHtmlHelper nopHtmlHelper, IGTMService gtm_Service,
            IPriceCalculationService priceCalculationService,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            IOrderService orderService,
            IGTMService gtmService,
            IGenericAttributeService genericAttributeService,
            ICategoryService categoryService,
            ICatalogModelFactory catalogModelFactory,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IProductService productService,
            ICurrencyService currencyService)
        {
            _nopHtmlHelper = nopHtmlHelper;
            _gtm_Service = gtm_Service;
            _priceCalculationService = priceCalculationService;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _orderService = orderService;
            _gtmService = gtmService;
            _genericAttributeService = genericAttributeService;
            _categoryService = categoryService;
            _catalogModelFactory = catalogModelFactory;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _productService = productService;
            _currencyService = currencyService;
        }

        private string FixIllegalJavaScriptChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("'", "\\'");
            return text;
        }

        private async Task<bool> IsPluginActive()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(store.Id);
            return googleTagManagerSettings.IsEnable;
        }

        static string HashEmail(string email)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(email));
                var hashEmail = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashEmail;
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginActive())
                return;

            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();

            if (controller.Equals("Checkout", StringComparison.InvariantCultureIgnoreCase) && (action.Equals("OpcConfirmOrder", StringComparison.InvariantCultureIgnoreCase)
                || action.Equals("Confirm", StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var item = eventMessage.Entity;
            if (item.ShoppingCartType == ShoppingCartType.Wishlist)
                return;

            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var script = @"
                <script>
                    window.dataLayer = window.dataLayer || [];
                    dataLayer.push({
                         'ecommerce':undefined
                    });
                    dataLayer.push({
                        'event': '%event_name%', 
                        'var_prodid': ['%product_id%'],
                        'var_pagetype' : '%page_type%',
                        'currency': '%currency%',
                        'var_prodval':%value%,
                        'ecommerce':%ecommerce%
                    });
                </script>";

            var ecommerceScript = await _gtm_Service.PrepareRemoveFromCartEcommerceAsync(item);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var sku = string.IsNullOrEmpty(product.Sku) ? product.Id.ToString() : product.Sku;
            var (finalPriceWithoutDiscounts, _, discountAmount, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, 0, true, item.Quantity);
            var totalPrice = (finalPriceWithoutDiscounts * item.Quantity) - discountAmount;
            totalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(totalPrice, currentCurrency);
            script = script.Replace("%ecommerce%", ecommerceScript);
            script = script.Replace("%event_name%", GoogleTagManagerDefaults.REMOVE_TO_CART);
            script = script.Replace("%product_id%", sku);
            script = script.Replace("%currency%", currentCurrency.CurrencyCode);
            script = script.Replace("%value%", totalPrice.ToString("0.00", CultureInfo.InvariantCulture));
            script = script.Replace("%page_type%", GoogleTagManagerDefaults.CART_PAGE);
            _nopHtmlHelper.AddInlineScriptParts(ResourceLocation.Footer, script);
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (!await IsPluginActive())
                return;

            var order = eventMessage.Order;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var currencyCode = currentCurrency.CurrencyCode;
            if (order != null && !await _genericAttributeService.GetAttributeAsync<bool>(order, ORDER_ALREADY_PROCESSED))
            {
                var orderScript = @"
                                    event: '%event_name%', 
                                    'var_prodid':[%product_ids%],
                                    'var_pagetype':'%page_type%',
                                    'var_prodvalue':%value%,
                                    'currency': '%currency%',
                                    'ecommerce':{%purchase_ecommerce%},
                                    'customer':{%customer_data%},
                                    'email_sha1':'%hashed_email%',
                                    'email':'%customer_email%'";

                var ids = new StringBuilder();
                foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    if (ids.Length > 0)
                        ids.Append(",");
                    ids.AppendLine("'" + await _gtmService.GetProductIdAsync(item.ProductId) + "'");
                }

                orderScript = orderScript.Replace("%event_name%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.PURCHASE));
                orderScript = orderScript.Replace("%page_type%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.PURCHASE));
                orderScript = orderScript.Replace("%product_ids%", ids.ToString());

                var orderSubtotalExclTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderSubtotalExclTax, currentCurrency);
                orderScript = orderScript.Replace("%value%", orderSubtotalExclTax.ToString("0.00", CultureInfo.InvariantCulture));
                orderScript = orderScript.Replace("%purchase_ecommerce%", await _gtmService.GetPurchaseEcommerceScriptAsync(order.Id));
                orderScript = orderScript.Replace("%customer_data%", await _gtmService.GetCustomerScriptAsync(customer.Id));
                orderScript = orderScript.Replace("%orderId%", FixIllegalJavaScriptChars(order.CustomOrderNumber));
                orderScript = orderScript.Replace("%total%", orderSubtotalExclTax.ToString("0.00", CultureInfo.InvariantCulture));
                orderScript = orderScript.Replace("%tax%", order.OrderTax.ToString("0.00", CultureInfo.InvariantCulture));

                var orderShipping = order.OrderShippingExclTax;
                orderShipping = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderShipping, currentCurrency);
                orderScript = orderScript.Replace("%shipping_cost%", orderShipping.ToString("0.00", CultureInfo.InvariantCulture));
                orderScript = orderScript.Replace("%currency%", currencyCode);
                orderScript = orderScript.Replace("%hashed_email%", FixIllegalJavaScriptChars(HashEmail(customer.Email)));
                orderScript = orderScript.Replace("%customer_email%", FixIllegalJavaScriptChars(customer.Email));

                await _genericAttributeService.SaveAttributeAsync(order, ORDER_ALREADY_PROCESSED, true);
                var key = string.Format(GoogleTagManagerDefaults.SessionKey, customer.CustomerGuid.ToString());
                _httpContextAccessor.HttpContext.Session.SetString(key, orderScript);
            }
        }

        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            if (!await IsPluginActive())
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext.GetRouteData();
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();
            var request = httpContext.Request;

            var viewMode = request.Query["viewmode"].ToString();
            var orderBy = request.Query["orderby"];
            var pageSize = request.Query["pagesize"];
            var pageNumber = request.Query["pagenumber"];
            if (controller.Equals("catalog", StringComparison.InvariantCultureIgnoreCase) && action.Equals("category", StringComparison.InvariantCultureIgnoreCase))
            {
                var categoryId = int.Parse(routeData.Values["categoryId"]?.ToString());
                var command = new CatalogProductsCommand();
                if (!string.IsNullOrEmpty(viewMode))
                    command.ViewMode = viewMode;
                if (int.TryParse(orderBy, out int orderby))
                    command.OrderBy = orderby;

                if (int.TryParse(pageSize, out int pagesize))
                    command.PageSize = pagesize;

                if (int.TryParse(pageNumber, out int pagenumber))
                    command.PageNumber = pagenumber;

                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                var model = await _catalogModelFactory.PrepareCategoryModelAsync(category, command);
                var products = model.CatalogProductsModel.Products;
                var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
                if (products.Count == 0)
                    return;
                var categoryScript = @"
                <script>
                    window.dataLayer = window.dataLayer || [];
                    dataLayer.push({
                        'event': '%event_name%', 
                        'var_prodid': [%product_ids%],
                        'var_pagetype' : '%page_type%',
                        'currency': '%currency%',
                        'ecommerce':%ecommerce%
                    });
                </script>
                ";
                var ids = new StringBuilder();
                foreach (var item in products)
                {
                    if (ids.Length > 0)
                        ids.Append(",");
                    var sku = item.Sku;
                    if (sku == null || string.IsNullOrEmpty(sku))
                        sku = item.Id.ToString();
                    ids.AppendLine("'" + FixIllegalJavaScriptChars(sku) + "'");
                }
                categoryScript = categoryScript.Replace("%event_name%", GoogleTagManagerDefaults.VIEW_ITEM_LIST);
                categoryScript = categoryScript.Replace("%page_type%", GoogleTagManagerDefaults.CATEGORY_VIEW);
                categoryScript = categoryScript.Replace("%product_ids%", ids.ToString());
                var ecommerceScript = await _gtmService.GetCategoryEcommerceScript(products);
                categoryScript = categoryScript.Replace("%ecommerce%", ecommerceScript);
                categoryScript = categoryScript.Replace("%currency%", currencyCode);
                _nopHtmlHelper.AddInlineScriptParts(ResourceLocation.Footer, categoryScript);
            }
            else if (controller.Equals("Common", StringComparison.InvariantCultureIgnoreCase) && action.Equals("ContactUs", StringComparison.InvariantCultureIgnoreCase))
            {
                var contactUsScript = @"
                <script>
                    window.dataLayer = window.dataLayer || [];
                    dataLayer.push({
                        'event': '%event_name%', 
                    });
                </script>
                ";
                contactUsScript = contactUsScript.Replace("%event_name%", GoogleTagManagerDefaults.CONTACT_US);
                _nopHtmlHelper.AddInlineScriptParts(ResourceLocation.Footer, contactUsScript);
            }
            else if (controller.Equals("Home", StringComparison.InvariantCultureIgnoreCase) && action.Equals("Index", StringComparison.InvariantCultureIgnoreCase))
            {
                var contactUsScript = @"
                <script>
                    window.dataLayer = window.dataLayer || [];
                    dataLayer.push({
                        'event': '%event_name%', 
                    });
                </script>";
                contactUsScript = contactUsScript.Replace("%event_name%", GoogleTagManagerDefaults.HOME_PAGE);
                _nopHtmlHelper.AddInlineScriptParts(ResourceLocation.Footer, contactUsScript);
            }
        }

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            if (!await IsPluginActive())
                return;

            var script = @"
                        'event': '%event_name%', 
                        'user_name': '%user_name%',
                        'user_email' : '%user_email%'";

            var user = eventMessage?.Customer;
            if (user == null)
                return;
            var customerName = await _customerService.GetCustomerFullNameAsync(user);
            script = script.Replace("%event_name%", GoogleTagManagerDefaults.CUSTOMER_REGISTER);
            script = script.Replace("%user_name%", FixIllegalJavaScriptChars(customerName));
            script = script.Replace("%user_email%", FixIllegalJavaScriptChars(user.Email));
            var key = string.Format(GoogleTagManagerDefaults.SessionKey, user.CustomerGuid.ToString());
            _httpContextAccessor.HttpContext.Session.SetString(key, script);
        }

        public async Task HandleEventAsync(ProductSearchEvent eventMessage)
        {
            if (!await IsPluginActive())
                return;

            var searchKey = eventMessage.SearchTerm;
            var script = @"
                <script>
                    window.dataLayer = window.dataLayer || [];
                    dataLayer.push({
                        'event': '%event_name%', 
                         'searchKey':'%search_key%'
                    });
                </script>";
            script = script.Replace("%event_name%", GoogleTagManagerDefaults.SEARCH);
            script = script.Replace("%search_key%", FixIllegalJavaScriptChars(searchKey));
            _nopHtmlHelper.AddInlineScriptParts(ResourceLocation.Footer, script);
            return;
        }
    }
}