using System;
using System.Collections.Generic;
using System.Linq;
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
using Nop.Services.Cms;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.UI;
using NopStation.Plugin.Widgets.PinterestAnalytics.Api;
using NopStation.Plugin.Widgets.PinterestAnalytics.Services;
using NopStation.Plugin.Widgets.PinteresteAnalytics;

namespace NopStation.Plugin.Widgets.PinterestAnalytics
{
    public class EventConsumer : IConsumer<ProductSearchEvent>,
        IConsumer<PageRenderingEvent>,
        IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
        IConsumer<CustomerLoggedinEvent>
    {
        #region Fields 

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly INopHtmlHelper _htmlHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IPinterestAnalyticsService _pinterestAnalyticsService;

        #endregion

        #region Ctor
        public EventConsumer(ICategoryService categoryService,
            IProductService productService,
            INopHtmlHelper htmlHelper,
            IWorkContext workContext,
            IWidgetPluginManager widgetPluginManager,
            IPinterestAnalyticsService pinterestAnalyticsService,
            IHttpContextAccessor httpContextAccessor)
        {
            _categoryService = categoryService;
            _productService = productService;
            _htmlHelper = htmlHelper;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _widgetPluginManager = widgetPluginManager;
            _pinterestAnalyticsService = pinterestAnalyticsService;
        }

        #endregion

        #region Utility

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
            return await _widgetPluginManager.IsPluginActiveAsync("Widgets.PinterestAnalytics");
        }

        public async Task<string> GetSearchPgaeVisitScript(string searchTerm)
        {
            var searchScrip = @"<script>
                pintrk('track', 'search', {
                lead_type: 'Search',
                property: '{CUSTOMER_EMAIL}',
                search_query: '{SEARCH_QUERY}',
               });
               </script>";
            var customer = await _workContext.GetCurrentCustomerAsync();
            searchScrip = searchScrip.Replace("{SEARCH_QUERY}", FixIllegalJavaScriptChars(searchTerm));
            searchScrip = searchScrip.Replace("{CUSTOMER_EMAIL}", FixIllegalJavaScriptChars(customer.Email));
            return searchScrip;
        }

        public async Task<string> GetPgaeVisitScriptAsync(int productId)
        {
            var pageVisitScript = @"<script>
                pintrk('track', 'pagevisit', {
                currency: '{CURRENCY}',
                lead_type: 'Product details',
                property: '{CUSTOMER_EMAIL}',
                content_ids:['{PRODUCT_ID}'],
                product_id: '{SKU}',
                line_items: [
                {
                    product_name: '{PRODUCT_NAME}',
                    product_id: '{SKU}',
                    product_category: '{CATEGORY}',
                    product_price: {PRICE},
                }
                ]
                });
                </script>";
            var product = await _productService.GetProductByIdAsync(productId);
            var category = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).FirstOrDefault();
            var categoryName = (await _categoryService.GetCategoryByIdAsync(category.CategoryId)).Name;
            var currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var customer = await _workContext.GetCurrentCustomerAsync();

            var sku = product.Sku;
            if (string.IsNullOrEmpty(sku))
                sku = product.Id.ToString();

            pageVisitScript = pageVisitScript.Replace("{CURRENCY}", FixIllegalJavaScriptChars(currency));
            pageVisitScript = pageVisitScript.Replace("{PRODUCT_NAME}", FixIllegalJavaScriptChars(product.Name));
            pageVisitScript = pageVisitScript.Replace("{SKU}", FixIllegalJavaScriptChars(sku));
            pageVisitScript = pageVisitScript.Replace("{CATEGORY}", FixIllegalJavaScriptChars(categoryName));
            pageVisitScript = pageVisitScript.Replace("{PRICE}", FixIllegalJavaScriptChars(product.Price.ToString()));
            pageVisitScript = pageVisitScript.Replace("{CUSTOMER_EMAIL}", FixIllegalJavaScriptChars(customer.Email));

            var productIds = new List<string> { sku };
            var ids = string.Join(",", productIds);
            pageVisitScript = pageVisitScript.Replace("{PRODUCT_ID}", FixIllegalJavaScriptChars(ids));

            return pageVisitScript;
        }

        public async Task<string> GetCategoryPgaeVisitScriptAsync(int categoryId)
        {
            var viewCategoryScript = @"<script>
                pintrk('track', 'viewcategory', {
                lead_type: 'Category page',
                content_ids:['{PRODUCT_ID}'],
                line_items: [
                {
                    product_id:'{PRODUCT_ID}',
                    product_category: '{CATEGORY_NAME}'
                }
                 ],
                });
                </script>";

            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            viewCategoryScript = viewCategoryScript.Replace("{CATEGORY_NAME}", FixIllegalJavaScriptChars(category.Name));
            viewCategoryScript = viewCategoryScript.Replace("{CUSTOMER_EMAIL}", FixIllegalJavaScriptChars(customer.Email));
            viewCategoryScript = viewCategoryScript.Replace("{PRODUCT_ID}", FixIllegalJavaScriptChars(categoryId.ToString()));
            return viewCategoryScript;
        }

        public static string GetSHA256Hash(string value)
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(byteArray);
        }

        #endregion

        #region Methods
        public async Task HandleEventAsync(ProductSearchEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var searchTerm = eventMessage.SearchTerm;
            if (searchTerm != null)
            {
                var searchScript = await GetSearchPgaeVisitScript(searchTerm);
                if (searchScript != null)
                    _htmlHelper.AddInlineScriptParts(ResourceLocation.Head, searchScript);
            }
            await Task.CompletedTask;
        }

        public async Task HandleEventAsync(PageRenderingEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext?.GetRouteData();
            if (routeData != null)
            {
                var actionName = routeData.Values["action"]?.ToString();
                var controllerName = routeData.Values["controller"]?.ToString();
                if (actionName == null || controllerName == null)
                    throw new Exception(typeof(PageRenderingEvent).Name);

                if (controllerName.Equals("Product", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("ProductDetails", StringComparison.OrdinalIgnoreCase))
                {
                    var productId = (int)routeData.Values["productId"];
                    var pageVisitScript = await GetPgaeVisitScriptAsync(productId);
                    _htmlHelper.AddInlineScriptParts(ResourceLocation.Head, pageVisitScript);
                }
                if (controllerName.Equals("catalog", StringComparison.OrdinalIgnoreCase) &&
                    actionName.Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    var categoryId = (int)routeData.Values["categoryId"];
                    var categoryPageVisitScript = await GetCategoryPgaeVisitScriptAsync(categoryId);
                    _htmlHelper.AddInlineScriptParts(ResourceLocation.Head, categoryPageVisitScript);
                }
            }
            await Task.CompletedTask;
        }

        public async Task<EventData> GetBaseEventScript(string eventName, string eventId)
        {
            var unixTime = Helpers.ConvertToUnixTimestamp(DateTime.UtcNow);
            var currentUser = await _workContext.GetCurrentCustomerAsync();
            var email = currentUser.Email;
            var hashedEmail = GetSHA256Hash(email);
            var eventData = new EventData
            {
                Event_Name = FixIllegalJavaScriptChars(eventName),
                Event_Id = FixIllegalJavaScriptChars(eventId),
                Action_Source = FixIllegalJavaScriptChars("web"),
                Event_Time = unixTime,
                User_Data = new UserData()
                {
                    Em = new List<string> {
                            FixIllegalJavaScriptChars(hashedEmail),
                        },
                    Client_Ip_Address = FixIllegalJavaScriptChars(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()),
                    Client_User_Agent = FixIllegalJavaScriptChars(_httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString())
                },
                Custom_Data = new CustomData(),
            };
            return eventData;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var item = eventMessage.Entity;
            var eventId = PinterestAnalyticsDefaults.ADD_TO_CART;
            var eventName = eventId;
            if (item.ShoppingCartType == ShoppingCartType.Wishlist)
            {
                eventId = PinterestAnalyticsDefaults.ADD_TO_WISHLIST;
                eventName = "custom";
            }
            var baseEvent = await GetBaseEventScript(eventName, eventId);
            var productPrice = (await _productService.GetProductByIdAsync(item.ProductId)).Price;
            var totalPrice = item.Quantity * productPrice;
            var customData = new CustomData()
            {
                Currency = FixIllegalJavaScriptChars("USD"),
                Value = totalPrice.ToString(),
                Contents = new List<Content>
                    {
                        new Content
                        {
                            Id = item.ProductId.ToString(),
                            Item_Price = productPrice.ToString(),
                            Quantity = item.Quantity,
                        }

                    },
                Num_Items = item.Quantity,
                Content_Ids = new List<string> { FixIllegalJavaScriptChars(item.ProductId.ToString()) }
            };
            baseEvent.Custom_Data = customData;
            await _pinterestAnalyticsService.SendAsync(baseEvent);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var action = _httpContextAccessor.HttpContext?.Request.RouteValues["action"]?.ToString();
            var controller = _httpContextAccessor.HttpContext?.Request.RouteValues["controller"]?.ToString();
            var isOrderCompletedPage = controller.ToString().Equals("Checkout", StringComparison.InvariantCultureIgnoreCase) &&
                        action.ToString().Equals("OpcConfirmOrder", StringComparison.InvariantCultureIgnoreCase);

            if (isOrderCompletedPage)
                return;

            var item = eventMessage.Entity;
            var eventName = "custom";
            var eventId = PinterestAnalyticsDefaults.REMOVE_FROM_CART;
            if (item.ShoppingCartType == ShoppingCartType.Wishlist)
                eventId = PinterestAnalyticsDefaults.REMOVE_FROM_WISHLIST;
            var baseEvent = await GetBaseEventScript(eventName, eventId);
            var productPrice = (await _productService.GetProductByIdAsync(item.ProductId)).Price;
            var totalPrice = item.Quantity * productPrice;
            var customData = new CustomData()
            {
                Currency = FixIllegalJavaScriptChars("USD"),
                Value = totalPrice.ToString(),
                Contents = new List<Content>
                    {
                        new Content
                        {
                            Id = item.ProductId.ToString(),
                            Item_Price = productPrice.ToString(),
                            Quantity = item.Quantity,
                        }

                    },
                Num_Items = item.Quantity,
                Content_Ids = new List<string> { FixIllegalJavaScriptChars(item.ProductId.ToString()) }
            };
            baseEvent.Custom_Data = customData;
            await _pinterestAnalyticsService.SendAsync(baseEvent);
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var item = eventMessage.Entity;
            var eventId = PinterestAnalyticsDefaults.ADD_TO_CART;
            var eventName = eventId;
            if (item.ShoppingCartType == ShoppingCartType.Wishlist)
            {
                eventId = PinterestAnalyticsDefaults.ADD_TO_WISHLIST;
                eventName = "custom";
            }
            var form = _httpContextAccessor.HttpContext.Request.Form;

            var formQuantity = form.ContainsKey($"addtocart_{item.ProductId}.EnteredQuantity") == true ?
                int.Parse(form[$"addtocart_{item.ProductId}.EnteredQuantity"]) : item.Quantity;
            var baseEvent = await GetBaseEventScript(eventName, eventId);
            var routeData = _httpContextAccessor.HttpContext?.GetRouteData();
            if (routeData != null)
            {
                var quantityValue = routeData.Values["quantity"];
                if (quantityValue != null && int.TryParse(quantityValue.ToString(), out var quantity))
                {
                    formQuantity = quantity;
                }
            }
            var productPrice = (await _productService.GetProductByIdAsync(item.ProductId)).Price;
            var totalPrice = formQuantity * productPrice;
            var customData = new CustomData()
            {
                Currency = FixIllegalJavaScriptChars("USD"),
                Value = totalPrice.ToString(),
                Contents = new List<Content>
                    {
                        new Content
                        {
                            Id = item.ProductId.ToString(),
                            Item_Price = productPrice.ToString(),
                            Quantity = formQuantity,
                        }

                    },
                Num_Items = formQuantity,
                Content_Ids = new List<string> { FixIllegalJavaScriptChars(item.ProductId.ToString()) }
            };
            baseEvent.Custom_Data = customData;
            await _pinterestAnalyticsService.SendAsync(baseEvent);
        }

        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            if (!await IsPluginEnabledAsync())
                return;

            var customer = eventMessage.Customer;
            var eventName = "custom";
            var eventId = PinterestAnalyticsDefaults.CUSTOMER_LOGIN;
            var baseEvent = await GetBaseEventScript(eventName, eventId);
            var customData = new CustomData()
            {
                Currency = FixIllegalJavaScriptChars("USD"),
                Value = customer.Email,
                Contents = new List<Content>(),
                Num_Items = 0,
                Content_Ids = new List<string>(),
            };
            baseEvent.Custom_Data = customData;
            await _pinterestAnalyticsService.SendAsync(baseEvent);
        }

        #endregion
    }
}