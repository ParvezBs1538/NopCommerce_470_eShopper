using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Services
{
    public class GTMService : IGTMService
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IGiftCardService _giftCardService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;

        public GTMService(IManufacturerService manufacturerService,
            ICategoryService categoryService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IOrderService orderService,
            IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IGiftCardService giftCardService,
            IProductService productService,
            ICurrencyService currencyService)
        {
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _orderService = orderService;
            _workContext = workContext;
            _customerService = customerService;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceCalculationService = priceCalculationService;
            _giftCardService = giftCardService;
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

        public async Task<string> GetCategoriesAsync(int productId)
        {
            var categories = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            var script = new StringBuilder();
            for (int i = 0; i < categories.Count; i++)
            {
                var categoryName = (await _categoryService.GetCategoryByIdAsync(categories[i].CategoryId)).Name;
                var categoryWithId = "item_category" + (i > 0 ? (i + 1).ToString() : "");
                var category = "'" + categoryWithId + "': '" + FixIllegalJavaScriptChars(categoryName) + "',";
                script.AppendLine(category);
            }

            return script.ToString();
        }

        public async Task<string> GetProductDetailsAsync(int productId, int quantity, int index = 0)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var itemInformationScript = @"{
                                        'item_id': '%item_id%',
                                        'item_name': '%productName%',
                                        'affiliation': '',
                                        'coupon':'%coupon_code%',
                                        'discount':%discount_amount%,
                                        'index':%index_number%,
                                        'item_brand': '%manufacturer%',  
                                         %categories%
                                        'price': %unitPrice%,
                                        'quantity': %quantity%,
                                        'copy': %quantity%,
                                    }";

            var manufacturer = "";
            var manufacturerModel = (await _manufacturerService.GetProductManufacturersByProductIdAsync(productId, true)).FirstOrDefault();
            if (manufacturerModel != null)
            {
                manufacturer = (await _manufacturerService.GetManufacturerByIdAsync(manufacturerModel.ManufacturerId))?.Name;
            }

            var product = await _productService.GetProductByIdAsync(productId);
            var sku = product.Sku;
            if (string.IsNullOrEmpty(sku))
                sku = productId.ToString();

            var (_, _, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, 0, true, quantity);
            discountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(discountAmount, currentCurrency);
            var categories = await GetCategoriesAsync(productId);
            itemInformationScript = itemInformationScript.Replace("%productName%", FixIllegalJavaScriptChars(product.Name));
            itemInformationScript = itemInformationScript.Replace("%categories%", categories);
            itemInformationScript = itemInformationScript.Replace("%manufacturer%", FixIllegalJavaScriptChars(manufacturer));
            itemInformationScript = itemInformationScript.Replace("%item_id%", FixIllegalJavaScriptChars(sku));
            itemInformationScript = itemInformationScript.Replace("%quantity%", FixIllegalJavaScriptChars(quantity.ToString()));
            itemInformationScript = itemInformationScript.Replace("%coupon_code%", FixIllegalJavaScriptChars(appliedDiscounts.FirstOrDefault()?.Name ?? ""));
            itemInformationScript = itemInformationScript.Replace("%discount_amount%", FixIllegalJavaScriptChars(discountAmount.ToString("0.00", CultureInfo.InvariantCulture)));
            itemInformationScript = itemInformationScript.Replace("%index_number%", index.ToString());

            return itemInformationScript;
        }

        public async Task<string> PrepareProductItemsAsync(IList<ShoppingCartItem> cart)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            var sb = new StringBuilder();
            var index = 0;
            foreach (var item in model.Items)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.AppendLine(",");
                var unitPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(item.UnitPriceValue, currentCurrency);
                var script = await GetProductDetailsAsync(item.ProductId, item.Quantity, index);
                script = script.Replace("%unitPrice%", unitPriceValue.ToString("0.00", CultureInfo.InvariantCulture));
                sb.AppendLine(script);
                index++;
            }

            return sb.ToString();
        }

        public async Task<string> PrepareOrderItemsAsync(int orderId)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var sb = new StringBuilder();
            var index = 0;
            foreach (var item in await _orderService.GetOrderItemsAsync(orderId))
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.AppendLine(",");
                var unitPriceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(item.UnitPriceExclTax, currentCurrency);
                var script = await GetProductDetailsAsync(item.ProductId, item.Quantity, index);
                script = script.Replace("%unitPrice%", unitPriceValue.ToString("0.00", CultureInfo.InvariantCulture));
                sb.AppendLine(script);
                index++;
            }

            return sb.ToString();
        }

        public async Task<string> PrepareRemoveFromCartEcommerceAsync(ShoppingCartItem cartItem)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var currency = currentCurrency.CurrencyCode;
            var removeFromCartScript = @"{
                                         'currency':'%currency%',
                                          'value':%value%,
                                           'price': %unitPrice%,
                                            items : [%items%]
                                         }";
            var items = await GetProductDetailsAsync(cartItem.ProductId, cartItem.Quantity);
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.Price, currentCurrency);
            removeFromCartScript = removeFromCartScript.Replace("%currency%", currency);
            removeFromCartScript = removeFromCartScript.Replace("%items%", items);
            removeFromCartScript = removeFromCartScript.Replace("%unitPrice%", price.ToString("0.00", CultureInfo.InvariantCulture));
            return removeFromCartScript;
        }

        public async Task<string> GetProductIdAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var sku = product.Sku;
            if (string.IsNullOrEmpty(sku))
                sku = productId.ToString();
            return FixIllegalJavaScriptChars(sku);
        }

        public async Task<string> GetPurchaseEcommerceScriptAsync(int orderId)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var orderEcommerceScript = @"
                                        'transaction_id': '%orderId%',
                                        'affiliation': '',
                                        'value': %total%,
                                        'tax': %tax%,
                                        'discount':%discount%,
                                        'shipping': %shipping_cost%,
                                        'currency': '%currency%',
                                        'coupon': '%coupon_code%',
                                        'items': [%productInformation%]
                                        ";

            var couponCode = "";
            var orderItemScript = await PrepareOrderItemsAsync(orderId);
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var usedGiftCard = (await _giftCardService.GetGiftCardUsageHistoryAsync(order)).FirstOrDefault();
            var orderDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderDiscount, currentCurrency);
            if (usedGiftCard != null)
            {
                couponCode = (await _giftCardService.GetGiftCardByIdAsync(usedGiftCard.GiftCardId)).GiftCardCouponCode;
            }
            orderEcommerceScript = orderEcommerceScript.Replace("%coupon_code%", couponCode);
            orderEcommerceScript = orderEcommerceScript.Replace("%discount%", orderDiscount.ToString("0.00", CultureInfo.InvariantCulture));
            orderEcommerceScript = orderEcommerceScript.Replace("%productInformation%", orderItemScript);
            return orderEcommerceScript;
        }

        public async Task<string> GetCustomerScriptAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            var customerName = await _customerService.GetCustomerFullNameAsync(customer);
            var sb = new StringBuilder();
            sb.Append("'user_fullName': '" + FixIllegalJavaScriptChars(customerName) + "'");
            sb.AppendLine(",");
            sb.Append("'user_id': '" + FixIllegalJavaScriptChars(customer.CustomerGuid.ToString()) + "'");
            return sb.ToString();
        }

        public async Task<string> GetCategoryEcommerceScript(IList<ProductOverviewModel> products)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var ecommerce = @"{
                              items : [%items%]  
                              }";
            var sb = new StringBuilder();
            var index = 0;
            foreach (var item in products)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.AppendLine(",");
                var script = await GetProductDetailsAsync(item.Id, 1, index);
                var value = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(item.ProductPrice.PriceValue ?? 0, currentCurrency);
                script = script.Replace("%unitPrice%", value.ToString("0.00", CultureInfo.InvariantCulture));
                sb.AppendLine(script);
                index++;
            }
            ecommerce = ecommerce.Replace("%items%", sb.ToString());
            return ecommerce;
        }

        public async Task<string> PrepareShoppingCartScriptAsync(Customer customer, int storeId)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var currencyCode = currentCurrency.CurrencyCode;
            if (cart.Any())
            {
                var (_, _, cartSubTotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
                cartSubTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(cartSubTotal, currentCurrency);
                var viewCartScript = GoogleTagManagerDefaults.BaseEventScript;
                var ids = new StringBuilder();
                foreach (var cartItem in cart)
                {
                    if (ids.Length > 0)
                        ids.Append(",");
                    ids.AppendLine("'" + await GetProductIdAsync(cartItem.ProductId) + "'");
                }

                viewCartScript = viewCartScript.Replace("%currency%", FixIllegalJavaScriptChars(currencyCode));
                viewCartScript = viewCartScript.Replace("%event_name%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.VIEW_CART));
                viewCartScript = viewCartScript.Replace("%page_type%", FixIllegalJavaScriptChars(GoogleTagManagerDefaults.CART_PAGE));
                viewCartScript = viewCartScript.Replace("%value%", cartSubTotal.ToString("0.00", CultureInfo.InvariantCulture));
                viewCartScript = viewCartScript.Replace("%total%", cartSubTotal.ToString("0.00", CultureInfo.InvariantCulture));
                viewCartScript = viewCartScript.Replace("%product_ids%", ids.ToString());
                var productList = await PrepareProductItemsAsync(cart);

                viewCartScript = viewCartScript.Replace("%productInformation%", productList);

                return viewCartScript;
            }

            return "";
        }
    }
}