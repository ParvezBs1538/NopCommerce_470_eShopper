using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Controllers
{
    public class GtmEventSendController : NopStationPublicController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly OrderSettings _orderSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public GtmEventSendController(IStoreContext storeContext,
            ISettingService settingService,
            IProductService productService,
            ICategoryService categoryService,
            ILogger logger,
            IWorkContext workContext,
            IManufacturerService manufacturerService,
            IShoppingCartService shoppingCartService,
            IPriceCalculationService priceCalculationService,
            OrderSettings orderSettings,
            ICurrencyService currencyService,
            IProductAttributeService productAttributeService,
            IShippingPluginManager shippingPluginManager,
            IPaymentPluginManager paymentPluginManager,
            ITaxService taxService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
            _workContext = workContext;
            _manufacturerService = manufacturerService;
            _shoppingCartService = shoppingCartService;
            _priceCalculationService = priceCalculationService;
            _orderSettings = orderSettings;
            _currencyService = currencyService;
            _productAttributeService = productAttributeService;
            _shippingPluginManager = shippingPluginManager;
            _paymentPluginManager = paymentPluginManager;
            _taxService = taxService;
        }

        #endregion

        #region Utilities

        private string FixIllegalJavaScriptChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("'", "\\'");
            return text;
        }

        private async Task<List<string>> GetCategoriesAsync(int productId)
        {
            var categoryList = await _categoryService.GetProductCategoriesByProductIdAsync(productId);
            var categories = new List<string>();
            foreach (var category in categoryList)
            {
                var cate = await _categoryService.GetCategoryByIdAsync(category.CategoryId);
                categories.Add(FixIllegalJavaScriptChars(cate.Name));
            }
            return categories;
        }

        private async Task<(List<object>, List<string>, decimal)> GetProducts()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var total = decimal.Zero;
            var products = new List<object>();
            var productIds = new List<string>();
            for (var i = 0; i < cart.Count; i++)
            {
                var product = cart[i];
                var productModel = await _productService.GetProductByIdAsync(product.ProductId);
                var manufacturer = "";
                var manufacturerModel = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true)).FirstOrDefault();
                if (manufacturerModel != null)
                {
                    manufacturer = (await _manufacturerService.GetManufacturerByIdAsync(manufacturerModel.ManufacturerId))?.Name;
                }

                var (finalPriceWithoutDiscounts, finalPrice, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(productModel, customer, store, 0, true, 1);
                var productObj = new
                {
                    Sku = FixIllegalJavaScriptChars(string.IsNullOrEmpty(productModel.Sku) == true ? productModel.Id.ToString() : productModel.Sku),
                    Name = FixIllegalJavaScriptChars(productModel.Name),
                    Affiliation = "",
                    Coupon = FixIllegalJavaScriptChars(appliedDiscounts.Count > 0 ? appliedDiscounts[0].Name : ""),
                    CurrencyCode = FixIllegalJavaScriptChars(currencyCode),
                    Discount = (discountAmount),
                    Index = (double)i,
                    Brand = FixIllegalJavaScriptChars(manufacturer),
                    Categories = await GetCategoriesAsync(product.ProductId),
                    ItemListId = "related_products",
                    ItemListName = "Related Products",
                    Price = (finalPriceWithoutDiscounts),
                    Quantity = product.Quantity,
                    Copy = product.Quantity,
                };
                total += finalPrice;

                productIds.Add(productObj.Sku);
                products.Add(productObj);
            }
            return (products, productIds, total);
        }

        private async Task<bool> IsPluginActive()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(store.Id);
            return googleTagManagerSettings.IsEnable;
        }

        private async Task<(object, string)> GetProductObjectAsync(int productId, int index)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var product = await _productService.GetProductByIdAsync(productId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var manufacturer = "";
            var manufacturerModel = (await _manufacturerService.GetProductManufacturersByProductIdAsync(productId, true)).FirstOrDefault();
            if (manufacturerModel != null)
            {
                manufacturer = (await _manufacturerService.GetManufacturerByIdAsync(manufacturerModel.ManufacturerId))?.Name;
            }

            var (finalPriceWithoutDiscounts, _, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, 0, true, 1);
            var productObj = new
            {
                Sku = FixIllegalJavaScriptChars(string.IsNullOrEmpty(product.Sku) == true ? product.Id.ToString() : product.Sku),
                Name = FixIllegalJavaScriptChars(product.Name),
                Affiliation = "",
                Coupon = FixIllegalJavaScriptChars(appliedDiscounts.Count > 0 ? appliedDiscounts[0].Name : ""),
                CurrencyCode = FixIllegalJavaScriptChars(currencyCode),
                Discount = (discountAmount),
                Index = (double)index,
                Brand = FixIllegalJavaScriptChars(manufacturer),
                Categories = await GetCategoriesAsync(product.Id),
                ItemListId = "related_products",
                ItemListName = "Related Products",
                Price = (finalPriceWithoutDiscounts),
                Quantity = 1,
            };
            return (productObj, productObj.Sku);
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int productId, bool isClickedFromProductDetailsPage, int quantity, bool isShoppingCart = true)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var googleTagManagerSettings = await _settingService.LoadSettingAsync<GoogleTagManagerSettings>(store.Id);

                if (!await IsPluginActive())
                    return Json(new { Result = false, Message = "Plugin Disabled!" });

                var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId);

                var cultureInfo = new CultureInfo("en-US");
                var product = await _productService.GetProductByIdAsync(productId);
                var allowedQuantities = _productService.ParseAllowedQuantities(product);
                var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);

                var isRedirect = false;
                if (product.ProductType != ProductType.SimpleProduct || product.OrderMinimumQuantity > 1 || product.CustomerEntersPrice
                    || product.IsRental || allowedQuantities.Length > 0 || productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
                {
                    isRedirect = true;
                }

                if (!isClickedFromProductDetailsPage && isRedirect)
                {
                    return Json(new { Result = false, Message = "Something wrong!" });
                }

                var manufacturer = "";
                var manufacturerModel = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true)).FirstOrDefault();
                if (manufacturerModel != null)
                {
                    manufacturer = (await _manufacturerService.GetManufacturerByIdAsync(manufacturerModel.ManufacturerId))?.Name;
                }

                var cartType = ShoppingCartType.ShoppingCart;
                if (!isShoppingCart)
                    cartType = ShoppingCartType.Wishlist;

                var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(customer, cartType, store.Id);
                var latestItem = shoppingCartItems
                                .OrderByDescending(item => item.CreatedOnUtc > item.UpdatedOnUtc ? item.CreatedOnUtc : item.UpdatedOnUtc)
                                .FirstOrDefault();

                if (quantity == 0)
                    quantity = product.OrderMinimumQuantity;
                var copy = quantity;

                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                decimal unitPriceValue = 0;
                if (product.CallForPrice && (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                {
                    unitPriceValue = 0;
                }
                else
                {
                    var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(latestItem, true)).unitPrice);
                    var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                    unitPriceValue = shoppingCartUnitPriceWithDiscount;
                }
                var price = unitPriceValue;
                var (finalPriceWithoutDiscounts, _, discountAmount, appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, 0, true, product.OrderMinimumQuantity);
                discountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(discountAmount, currentCurrency);
                var productObj = new
                {
                    Name = FixIllegalJavaScriptChars(product.Name),
                    Sku = FixIllegalJavaScriptChars(string.IsNullOrEmpty(product.Sku) == true ? product.Id.ToString() : product.Sku),
                    Affiliation = "",
                    Coupon = FixIllegalJavaScriptChars(appliedDiscounts.Count > 0 ? appliedDiscounts[0].Name : ""),
                    Discount = discountAmount,
                    Index = 0.00,
                    Manufacturer = FixIllegalJavaScriptChars(manufacturer),
                    Categories = await GetCategoriesAsync(productId),
                    Price = price,
                    Quaantity = copy,
                    Currency = currentCurrency.CurrencyCode,
                    Copy = copy
                };

                return Json(new { Result = true, Data = productObj, Price = ((unitPriceValue * copy) - discountAmount).ToString("0.00") });
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "Error in GTM tracking: ", ex.ToString());
                return Json(new { Result = false, Message = "Something wrong!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ShoppingCartDetails(string systemName = "")
        {
            if (!await IsPluginActive())
                return Json(new { Result = false, Message = "Plugin Disabled!" });

            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;
            var (products, productIds, total) = await GetProducts();
            var shippingPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(systemName);
            var paymentPlugin = await _paymentPluginManager.LoadPluginBySystemNameAsync(systemName);
            var name = string.Empty;
            if (shippingPlugin != null)
                name = shippingPlugin.PluginDescriptor.FriendlyName;
            if (paymentPlugin != null)
                name = paymentPlugin.PluginDescriptor.FriendlyName;
            return Json(new
            {
                Result = true,
                Products = products,
                ProductIds = productIds,
                Currency = currencyCode,
                Total = total,
                Name = name,
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetProducts(IList<int> productIds)
        {
            if (!await IsPluginActive())
                return Json(new { Result = false, Message = "Plugin Disabled!" });

            var products = new List<object>();
            var productIDs = new List<string>();
            for (int i = 0; i < productIds.Count; i++)
            {
                var (productObject, sku) = await GetProductObjectAsync(productIds[i], i);

                productIDs.Add(sku);
                products.Add(productObject);
            }
            return Json(new
            {
                Result = true,
                ProductIds = productIDs,
                Products = products
            });
        }

        #endregion
    }
}