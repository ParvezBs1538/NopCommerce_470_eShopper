using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.GoogleAnalytics.Api;
using NopStation.Plugin.Widgets.GoogleAnalytics.Models;
using NopStation.Plugin.Widgets.GoogleAnalytics.Services;

namespace NopStation.Plugin.Widgets.GoogleAnalytics
{
    public class EventConsumer : IConsumer<OrderStatusChangedEvent>,
        IConsumer<OrderPaidEvent>,
        IConsumer<EntityDeletedEvent<Order>>,
        IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
        IConsumer<CustomerRegisteredEvent>,
        IConsumer<CustomerLoggedinEvent>,
        IConsumer<ModelPreparedEvent<BaseNopModel>>,
        IConsumer<PageRenderingEvent>,
        IConsumer<ProductSearchEvent>,
        IConsumer<OrderRefundedEvent>,
        IConsumer<OrderPlacedEvent>
    {
        private const string ORDER_ALREADY_PROCESSED_ATTRIBUTE_NAME = "GoogleAnalytics.OrderAlreadyProcessed";
        private readonly IAddressService _addressService;
        private readonly ICategoryService _categoryService;
        private readonly ICountryService _countryService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INSGoogleAnalyticsService _nSGoogleAnalyticsService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly INopStationContext _nopStationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OrderSettings _orderSettings;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly NopstationGoogleAnalyticsSettings _googleAnalyticsSettings;

        public EventConsumer(IAddressService addressService,
            ICategoryService categoryService,
            ICountryService countryService,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            ICustomerService customerService,
            NopstationGoogleAnalyticsSettings googleAnalyticsSettings,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            ISettingService settingService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWebHelper webHelper,
            IWidgetPluginManager widgetPluginManager,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IManufacturerService manufacturerService,
            INSGoogleAnalyticsService nSGoogleAnalyticsService,
            IProductAttributeFormatter productAttributeFormatter,
            INopStationContext nopStationContext,
            IHttpContextAccessor httpContextAccessor,
            OrderSettings orderSettings)
        {
            _addressService = addressService;
            _categoryService = categoryService;
            _countryService = countryService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _webHelper = webHelper;
            _widgetPluginManager = widgetPluginManager;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _manufacturerService = manufacturerService;
            _nSGoogleAnalyticsService = nSGoogleAnalyticsService;
            _productAttributeFormatter = productAttributeFormatter;
            _nopStationContext = nopStationContext;
            _httpContextAccessor = httpContextAccessor;
            _orderSettings = orderSettings;
            _customerService = customerService;
            _googleAnalyticsSettings = googleAnalyticsSettings;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        #region Utilities

        private async Task<string> GetEcommerceScriptAsync(Order order)
        {
            var analyticsTrackingScript = _googleAnalyticsSettings.TrackingScript + "\n";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{GOOGLEID}", _googleAnalyticsSettings.GoogleId);
            //remove {ECOMMERCE} (used in previous versions of the plugin)
            analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE}", "");
            //remove {CustomerID} (used in previous versions of the plugin)
            analyticsTrackingScript = analyticsTrackingScript.Replace("{CustomerID}", "");

            //whether to include customer identifier
            var customerIdCode = string.Empty;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (_googleAnalyticsSettings.IncludeCustomerId && !await _customerService.IsGuestAsync(customer))
                customerIdCode = $"gtag('set', {{'user_id': '{customer.Id}'}});{Environment.NewLine}";
            analyticsTrackingScript = analyticsTrackingScript.Replace("{CUSTOMER_TRACKING}", customerIdCode);

            //ecommerce info
            var store = await _storeContext.GetCurrentStoreAsync();
            var googleAnalyticsSettings = await _settingService.LoadSettingAsync<NopstationGoogleAnalyticsSettings>(store.Id);
            //ensure that ecommerce tracking code is renderred only once (avoid duplicated data in Google Analytics)
            if (order != null && !await _genericAttributeService.GetAttributeAsync<bool>(order, ORDER_ALREADY_PROCESSED_ATTRIBUTE_NAME))
            {
                var usCulture = new CultureInfo("en-US");

                var analyticsEcommerceScript = @"gtag('event', 'purchase', {
                    'transaction_id': '{ORDERID}',
                    'affiliation': '{SITE}',
                    'value': {TOTAL},
                    'currency': '{CURRENCY}',
                    'tax': {TAX},
                    'shipping': {SHIP},
                    'payment_method' : '{PAYMENT_METHOD}',
                    'shipping_method' : '{SHIPPING_METHOD}',
                    'items': [
                        {DETAILS}
                    ]
                })";

                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{ORDERID}", FixIllegalJavaScriptChars(order.CustomOrderNumber));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SITE}", FixIllegalJavaScriptChars(store.Name));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TOTAL}", order.OrderTotal.ToString("0.00", usCulture));
                var currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{CURRENCY}", currencyCode);
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{TAX}", order.OrderTax.ToString("0.00", usCulture));
                var orderShipping = googleAnalyticsSettings.IncludingTax ? order.OrderShippingInclTax : order.OrderShippingExclTax;
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SHIP}", orderShipping.ToString("0.00", usCulture));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{PAYMENT_METHOD}", FixIllegalJavaScriptChars(order.PaymentMethodSystemName));
                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{SHIPPING_METHOD}", FixIllegalJavaScriptChars(order.ShippingMethod));

                var sb = new StringBuilder();
                var listingPosition = 1;
                foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    if (!string.IsNullOrEmpty(sb.ToString()))
                        sb.AppendLine(",");

                    var analyticsEcommerceDetailScript = @"{
                        'item_id': '{PRODUCTSKU}',
                        'item_name': '{PRODUCTNAME}',
                        'item_category': '{CATEGORYNAME}',
                        'list_position': {LISTPOSITION},
                        'quantity': {QUANTITY},
                        'price': {UNITPRICE}
                    }";

                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);

                    if (string.IsNullOrEmpty(sku))
                        sku = product.Id.ToString();

                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTSKU}", FixIllegalJavaScriptChars(sku));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{PRODUCTNAME}", FixIllegalJavaScriptChars(product.Name));
                    var category = (await _categoryService.GetCategoryByIdAsync((await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).FirstOrDefault()?.CategoryId ?? 0))?.Name;
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{CATEGORYNAME}", FixIllegalJavaScriptChars(category));
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{LISTPOSITION}", listingPosition.ToString());
                    var unitPrice = googleAnalyticsSettings.IncludingTax ? item.UnitPriceInclTax : item.UnitPriceExclTax;
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{QUANTITY}", item.Quantity.ToString());
                    analyticsEcommerceDetailScript = analyticsEcommerceDetailScript.Replace("{UNITPRICE}", unitPrice.ToString("0.00", usCulture));
                    sb.AppendLine(analyticsEcommerceDetailScript);

                    listingPosition++;
                }

                analyticsEcommerceScript = analyticsEcommerceScript.Replace("{DETAILS}", sb.ToString());
                analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE_TRACKING}", analyticsEcommerceScript);

                await _genericAttributeService.SaveAttributeAsync(order, ORDER_ALREADY_PROCESSED_ATTRIBUTE_NAME, true);
            }
            else
            {
                analyticsTrackingScript = analyticsTrackingScript.Replace("{ECOMMERCE_TRACKING}", "");
            }

            return analyticsTrackingScript;
        }

        private string FixIllegalJavaScriptChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            //replace ' with \' (http://stackoverflow.com/questions/4292761/need-to-url-encode-labels-when-tracking-events-with-google-analytics)
            text = text.Replace("'", "\\'");
            return text;
        }

        private async Task<bool> IsPluginEnabledAsync()
        {
            return await _widgetPluginManager.IsPluginActiveAsync("NopStation.Plugin.Widgets.GoogleAnalytics");
        }

        public async Task<ProductItem> GetProductItemById(int productId, int storeId, int quantity, string variant)
        {
            var store = await _storeService.GetStoreByIdAsync(storeId);

            var product = await _productService.GetProductByIdAsync(productId);
            var category = (await _categoryService.GetCategoryByIdAsync(
                (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).FirstOrDefault()?.CategoryId ?? 0))?.Name ?? "";
            var manufacturer = (await _manufacturerService.GetManufacturerByIdAsync(
                (await _manufacturerService.GetProductManufacturersByProductIdAsync(productId)).FirstOrDefault()?.ManufacturerId ?? 0))?.Name ?? "";
            var priceModel = await _nSGoogleAnalyticsService.PrepareProductPriceModelAsync(product);
            var priceWithDiscount = priceModel?.PriceWithDiscountValue ?? product.Price;

            var item = new ProductItem();
            var productSku = product.Sku;
            if (string.IsNullOrEmpty(productSku))
                productSku = product.Id.ToString();

            item.item_id = FixIllegalJavaScriptChars(productSku);
            item.item_name = FixIllegalJavaScriptChars(product.Name);
            item.affiliation = FixIllegalJavaScriptChars(store.Name);
            item.price = product.Price;
            item.discount = product.Price - priceWithDiscount;
            item.item_category = FixIllegalJavaScriptChars(category);
            item.item_brand = manufacturer;
            item.item_variant = variant;
            item.quantity = quantity;

            return item;
        }

        #endregion

        #region Default Methods

        private async Task ProcessOrderEventAsync(Order order, bool add)
        {
            try
            {
                //settings per store
                var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
                var googleAnalyticsSettings = await _settingService.LoadSettingAsync<NopstationGoogleAnalyticsSettings>(store.Id);

                var request = new GoogleRequest
                {
                    AccountCode = googleAnalyticsSettings.GoogleId,
                    Culture = "en-US",
                    HostName = new Uri(_webHelper.GetThisPageUrl(false)).Host,
                    PageTitle = add ? "AddTransaction" : "CancelTransaction"
                };

                var orderId = order.CustomOrderNumber;
                var orderShipping = googleAnalyticsSettings.IncludingTax ? order.OrderShippingInclTax : order.OrderShippingExclTax;
                var orderTax = order.OrderTax;
                var orderTotal = order.OrderTotal;
                if (!add)
                {
                    orderShipping = -orderShipping;
                    orderTax = -orderTax;
                    orderTotal = -orderTotal;
                }

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var trans = new Transaction(FixIllegalJavaScriptChars(orderId),
                    FixIllegalJavaScriptChars(billingAddress.City),
                    await _countryService.GetCountryByAddressAsync(billingAddress) is Country country ? FixIllegalJavaScriptChars(country.Name) : string.Empty,
                    await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince stateProvince ? FixIllegalJavaScriptChars(stateProvince.Name) : string.Empty,
                    store.Name,
                    orderShipping,
                    orderTax,
                    orderTotal);

                foreach (var item in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    //get category
                    var category = (await _categoryService.GetCategoryByIdAsync((await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault()?.CategoryId ?? 0))?.Name;
                    if (string.IsNullOrEmpty(category))
                        category = "No category";

                    var unitPrice = googleAnalyticsSettings.IncludingTax ? item.UnitPriceInclTax : item.UnitPriceExclTax;
                    var qty = item.Quantity;
                    if (!add)
                        qty = -qty;

                    var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                    if (string.IsNullOrEmpty(sku))
                        sku = product.Id.ToString();

                    var productItem = new TransactionItem(FixIllegalJavaScriptChars(orderId),
                      FixIllegalJavaScriptChars(sku),
                      FixIllegalJavaScriptChars(product.Name),
                      unitPrice,
                      qty,
                      FixIllegalJavaScriptChars(category));

                    trans.Items.Add(productItem);
                }

                await request.SendRequest(trans, _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient));
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Google Analytics. Error canceling transaction from server side", ex.ToString());
            }
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var order = eventMessage.Order;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var orderCompletedScript = await GetEcommerceScriptAsync(order);

            var key = string.Format(EventNameDefaults.SESSION_KEY_ORDER_SCRIPT, customer.Id);
            _httpContextAccessor.HttpContext.Session.SetString(key, orderCompletedScript);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            //ensure the plugin is installed and active
            if (!await IsPluginEnabledAsync())
                return;

            var order = eventMessage.Entity;

            //settings per store
            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var googleAnalyticsSettings = await _settingService.LoadSettingAsync<NopstationGoogleAnalyticsSettings>(store.Id);

            //ecommerce is disabled
            if (!googleAnalyticsSettings.EnableEcommerce)
                return;

            bool sendRequest;
            if (googleAnalyticsSettings.UseJsToSendEcommerceInfo)
            {
                //if we use JS to notify GA about new orders (even when they are placed), then we should always notify GA about deleted orders
                //but ignore already cancelled orders (do not duplicate request to GA)
                sendRequest = order.OrderStatus != OrderStatus.Cancelled;
            }
            else
            {
                //if we use HTTP requests to notify GA about new orders (only when they are paid), then we should notify GA about deleted AND paid orders
                sendRequest = order.PaymentStatus == PaymentStatus.Paid;
            }

            if (sendRequest)
                await ProcessOrderEventAsync(order, false);
        }

        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            //ensure the plugin is installed and active
            if (!await IsPluginEnabledAsync())
                return;

            var order = eventMessage.Order;

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                //settings per store
                var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
                var googleAnalyticsSettings = await _settingService.LoadSettingAsync<NopstationGoogleAnalyticsSettings>(store.Id);

                //ecommerce is disabled
                if (!googleAnalyticsSettings.EnableEcommerce)
                    return;

                //if we use JS to notify GA about new orders (even when they are placed), then we should always notify GA about deleted orders
                //if we use HTTP requests to notify GA about new orders (only when they are paid), then we should notify GA about deleted AND paid orders
                var sendRequest = googleAnalyticsSettings.UseJsToSendEcommerceInfo || order.PaymentStatus == PaymentStatus.Paid;

                if (sendRequest)
                    await ProcessOrderEventAsync(order, false);
            }
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            //ensure the plugin is installed and active
            if (!await IsPluginEnabledAsync())
                return;

            var order = eventMessage.Order;

            //settings per store
            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var googleAnalyticsSettings = await _settingService.LoadSettingAsync<NopstationGoogleAnalyticsSettings>(store.Id);

            //ecommerce is disabled
            if (!googleAnalyticsSettings.EnableEcommerce)
                return;

            //we use HTTP requests to notify GA about new orders (only when they are paid)
            var sendRequest = !googleAnalyticsSettings.UseJsToSendEcommerceInfo;

            if (sendRequest)
                await ProcessOrderEventAsync(order, true);
        }

        #endregion

        #region by_measurement_api

        #region Shopping & Wishlist

        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
            var priceModel = await _nSGoogleAnalyticsService.PrepareProductPriceModelAsync(product);

            var attributesAll = await _productAttributeFormatter.FormatAttributesAsync(product, eventMessage.Entity.AttributesXml);
            var attribute = attributesAll.Split('<')[0];
            // check shopping cart controller for discounted price
            var items = new List<ProductItem>
            {
                await GetProductItemById(product.Id, eventMessage.Entity.StoreId, eventMessage.Entity.Quantity, attribute)
            };
            var eventParam = new
            {
                currency = currencyCode,
                user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                value = priceModel?.PriceWithDiscountValue ?? product.Price,
                items = items
            };

            var eventName = EventNameDefaults.ADD_TO_CART;
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.Wishlist)
                eventName = EventNameDefaults.ADD_TO_WISHLIST;

            await _nSGoogleAnalyticsService.PostAsync(eventParam, eventName);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
            var priceModel = await _nSGoogleAnalyticsService.PrepareProductPriceModelAsync(product);

            var attributesAll = await _productAttributeFormatter.FormatAttributesAsync(product, eventMessage.Entity.AttributesXml);
            var attribute = attributesAll.Split('<')[0];
            // check shopping cart controller for discounted price
            var items = new List<ProductItem>
            {
                await GetProductItemById(product.Id, eventMessage.Entity.StoreId, eventMessage.Entity.Quantity, attribute)
            };
            var eventParam = new
            {
                currency = currencyCode,
                user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                value = priceModel?.PriceWithDiscountValue ?? product.Price,
                items = items
            };

            var eventName = EventNameDefaults.ADD_TO_CART;
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.Wishlist)
                eventName = EventNameDefaults.ADD_TO_WISHLIST;

            await _nSGoogleAnalyticsService.PostAsync(eventParam, eventName);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var routeName = httpContext.GetEndpoint()?.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName;
            if (routeName != "ShoppingCart")
                return;

            if (!await IsPluginEnabledAsync())
                return;

            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var product = await _productService.GetProductByIdAsync(eventMessage.Entity.ProductId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
            var priceModel = await _nSGoogleAnalyticsService.PrepareProductPriceModelAsync(product);

            var attributesAll = await _productAttributeFormatter.FormatAttributesAsync(product, eventMessage.Entity.AttributesXml);
            var attribute = attributesAll.Split('<')[0];
            // check shopping cart controller for discounted price
            var items = new List<ProductItem>
            {
                await GetProductItemById(product.Id, eventMessage.Entity.StoreId, eventMessage.Entity.Quantity, attribute)
            };
            var eventParam = new
            {
                currency = currencyCode,
                user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                value = priceModel?.PriceWithDiscountValue ?? product.Price,
                items = items
            };

            var eventName = EventNameDefaults.REMOVE_TO_CART;
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.Wishlist)
                eventName = EventNameDefaults.REMOVE_TO_WISHLIST;

            await _nSGoogleAnalyticsService.PostAsync(eventParam, eventName);
        }

        #endregion

        public async Task HandleEventAsync(ProductSearchEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");

            var eventParams = new
            {
                term = eventMessage.SearchTerm,
                user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                user_phone = !string.IsNullOrEmpty(phone) ? phone : ""
            };
            await _nSGoogleAnalyticsService.PostAsync(eventParams, EventNameDefaults.SEARCH);
        }

        public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var eventParams = new
            {
                id = eventMessage.Customer?.Id,
                email = eventMessage.Customer?.Email,
                username = eventMessage.Customer?.Username
            };
            await _nSGoogleAnalyticsService.PostAsync(eventParams, EventNameDefaults.CUSTOMER_REGISTER);
        }

        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            if ((await _nopStationContext.GetRouteNameAsync()).Equals("Category", StringComparison.InvariantCultureIgnoreCase))
            {
                // category view
                var categoryId = _nopStationContext.GetRouteValue(NopRoutingDefaults.RouteValue.CategoryId, 0);
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);

                var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");

                var eventParam = new
                {
                    id = category?.Id,
                    name = category?.Name,
                    user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                    user_phone = !string.IsNullOrEmpty(phone) ? phone : ""
                };
                await _nSGoogleAnalyticsService.PostAsync(eventParam, EventNameDefaults.CATEGORY_VIEW);
            }

            if ((await _nopStationContext.GetRouteNameAsync()).Equals("ShoppingCart", StringComparison.InvariantCultureIgnoreCase))
            {
                var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return;

                var items = new List<ProductItem>();
                foreach (var item in cart)
                {
                    items.Add(await GetProductItemById(item.ProductId, store.Id, item.Quantity, item.AttributesXml));
                }

                var (_, _, cartTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);

                var eventParam = new
                {
                    currency = currencyCode,
                    user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                    user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                    value = cartTotal,
                    items = items
                };

                await _nSGoogleAnalyticsService.PostAsync(eventParam, EventNameDefaults.VIEW_CART);
            }

            if ((await _nopStationContext.GetRouteNameAsync()).Equals("CheckoutOnePage", StringComparison.InvariantCultureIgnoreCase) || (await _nopStationContext.GetRouteNameAsync()).Equals("CheckoutBillingAddress", StringComparison.InvariantCultureIgnoreCase))
            {
                if (_orderSettings.CheckoutDisabled)
                    return;

                var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return;

                var items = new List<ProductItem>();
                foreach (var item in cart)
                {
                    items.Add(await GetProductItemById(item.ProductId, store.Id, item.Quantity, item.AttributesXml));
                }

                var (_, _, cartTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);

                var eventParam = new
                {
                    currency = currencyCode,
                    user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                    user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                    value = cartTotal,
                    items = items
                };

                await _nSGoogleAnalyticsService.PostAsync(eventParam, EventNameDefaults.BEGIN_CHECKOUT);
            }
        }

        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var phone = await _genericAttributeService.GetAttributeAsync<string>(eventMessage.Customer, "Phone");
            var eventParams = new
            {
                id = eventMessage.Customer.Id,
                username = eventMessage.Customer.Username,
                email = eventMessage.Customer.Email,
                user_phone = !string.IsNullOrEmpty(phone) ? phone : ""
            };
            await _nSGoogleAnalyticsService.PostAsync(eventParams, EventNameDefaults.CUSTOMER_LOGIN);
        }

        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            if (eventMessage?.Model is ProductDetailsModel productDetailsModel)
            {
                var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
                var product = await _productService.GetProductByIdAsync(productDetailsModel.Id);
                var customer = await _workContext.GetCurrentCustomerAsync();
                var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");
                var priceModel = await _nSGoogleAnalyticsService.PrepareProductPriceModelAsync(product);
                var store = await _storeContext.GetCurrentStoreAsync();

                var attributesAll = await _productAttributeFormatter.FormatAttributesAsync(product, "");
                var attribute = attributesAll.Split('<')[0];
                // check shopping cart controller for discounted price
                var items = new List<ProductItem>
                {
                    await GetProductItemById(product.Id, store.Id, 1, attribute)
                };
                var eventParam = new
                {
                    currency = currencyCode,
                    user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                    user_phone = !string.IsNullOrEmpty(phone) ? phone : "",
                    value = priceModel?.PriceWithDiscountValue ?? product.Price,
                    items = items
                };

                var eventName = EventNameDefaults.VIEW_ITEM;
                await _nSGoogleAnalyticsService.PostAsync(eventParam, eventName);
            }
        }

        public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, "Phone");

            var eventParams = new
            {
                transaction_id = eventMessage.Order.Id,
                value = eventMessage.Amount,
                customerid = eventMessage.Order.CustomerId,
                user_email = !string.IsNullOrEmpty(customer.Email) ? customer.Email : "",
                user_phone = !string.IsNullOrEmpty(phone) ? phone : ""
            };
            await _nSGoogleAnalyticsService.PostAsync(eventParams, EventNameDefaults.ORDER_REFUND);
        }

        #endregion
    }
}