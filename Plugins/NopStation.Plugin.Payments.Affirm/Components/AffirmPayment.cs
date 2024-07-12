using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Tax;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Extensions;
using NopStation.Plugin.Payments.Affirm.Models;

namespace NopStation.Plugin.Payments.Affirm.Components
{
    public class AffirmPaymentViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IProductService _productService;
        private readonly ITaxService _taxService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ICategoryService _categoryService;
        private readonly AffirmPaymentSettings _affirmPaymentSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;

        private readonly string _sandboxJsURL = "https://cdn1-sandbox.affirm.com/js/v2/affirm.js";
        private readonly string _canadaSandboxJsURL = "https://cdn1-sandbox.affirm.ca/js/v2/affirm.js";
        private readonly string _jsURL = "https://cdn1.affirm.com/js/v2/affirm.js";
        private readonly string _canadaJsURL = "https://cdn1.affirm.ca/js/v2/affirm.js";

        #endregion

        #region Ctor

        public AffirmPaymentViewComponent(IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductService productService,
            ITaxService taxService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            ICategoryService categoryService,
            AffirmPaymentSettings affirmPaymentSettings,
            IShoppingCartService shoppingCartService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService)
        {
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderTotalCalculationService = orderTotalCalculationService;
            _productService = productService;
            _taxService = taxService;
            _pictureService = pictureService;
            _currencyService = currencyService;
            _categoryService = categoryService;
            _affirmPaymentSettings = affirmPaymentSettings;
            _shoppingCartService = shoppingCartService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!AffirmExtension.PluginActive())
                return Content("");

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                throw new NopException("Cart is empty");

            //customer currency
            var currentCustomerCurrencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId;
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);

            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

            if (customerCurrency == null)
                throw new NopException("Currency is not available");

            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            if (!shoppingCartTotal.shoppingCartTotal.HasValue)
                return Content("");

            //shipping total
            var orderShippingTotalInclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true);
            var orderShippingTotalExclTax = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, false);
            if (!orderShippingTotalInclTax.shippingTotal.HasValue || !orderShippingTotalExclTax.shippingTotal.HasValue)
                throw new NopException("Shipping total couldn't be calculated");

            var taxAmount = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
            var shippingAddressModel = PrepareAddressModelAsync((await _workContext.GetCurrentCustomerAsync()).ShippingAddressId);
            var billingAddressModel = PrepareAddressModelAsync((await _workContext.GetCurrentCustomerAsync()).BillingAddressId);
            var itemsModel = PrepareItemsModelAsync(cart);
            var merchant = new MerchantModel
            {
                // as we dont't redirect to affirm site
                UserCancelUrl = "https://www.affirm.com",
                UserConfirmationUrl = "https://www.affirm.com",
                UserConfirmationUrlAction = "GET",
                Merchant = _affirmPaymentSettings.MerchantName
            };
            var metadata = new MetadataModel
            {
                Mode = "modal"
            };

            var affirmJsonModel = new JsonRootModel()
            {
                Merchant = merchant,
                ShippingAmount = orderShippingTotalInclTax.shippingTotal.Value,
                TaxAmount = taxAmount.taxTotal,
                Shipping = await shippingAddressModel,
                Billing = await billingAddressModel,
                Metadata = metadata,
                Items = await itemsModel,
                Total = decimal.ToInt32(shoppingCartTotal.shoppingCartTotal.Value * 100),
                OrderId = Guid.NewGuid().ToString(),
                Currency = customerCurrency.CurrencyCode
            };

            string jsUrl;
            if (_affirmPaymentSettings.CountryAPIMode == CountryAPIMode.USA)
                jsUrl = _affirmPaymentSettings.UseSandbox ? _sandboxJsURL : _jsURL;
            else
                jsUrl = _affirmPaymentSettings.UseSandbox ? _canadaSandboxJsURL : _canadaJsURL;

            var model = new PaymentInfoModel
            {
                AffirmJSON = JsonConvert.SerializeObject(affirmJsonModel),
                PublicApiKey = _affirmPaymentSettings.PublicApiKey,
                JsURL = jsUrl
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Affirm/Views/PaymentInfo.cshtml", model);
        }

        private async Task<List<ItemModel>> PrepareItemsModelAsync(IList<ShoppingCartItem> cart)
        {
            var list = new List<ItemModel>();
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var productUnitPrice = await _shoppingCartService.GetUnitPriceAsync(sci, true);
                var shoppingCartUnitPriceWithDiscountBase = await _taxService.GetProductPriceAsync(product, productUnitPrice.unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase.price, await _workContext.GetWorkingCurrencyAsync());

                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(sci.ProductId);
                var categories = new List<List<string>>();
                foreach (var category in productCategories)
                {
                    categories.Add(await PrepareCategoryParentsModelAsync(category));
                }

                list.Add(new ItemModel()
                {
                    Qty = sci.Quantity,
                    Sku = product.Sku,
                    DisplayName = product.Name,
                    UnitPrice = decimal.ToInt32(shoppingCartUnitPriceWithDiscount * 100)
                });
            }

            return list;
        }

        private async Task<List<string>> PrepareCategoryParentsModelAsync(ProductCategory productCategory)
        {
            var list = new List<string>();
            var category = await _categoryService.GetCategoryByIdAsync(productCategory.CategoryId);

            while (category != null && !category.Deleted)
            {
                list.Insert(0, await _localizationService.GetLocalizedAsync(category, x => x.Name));

                category = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
            }

            return list;
        }

        private async Task<AddressDetailsModel> PrepareAddressModelAsync(int? addressId)
        {
            if (!addressId.HasValue)
                return null;

            var address = await _addressService.GetAddressByIdAsync(addressId.Value);
            if (address == null)
                return null;

            return new AddressDetailsModel()
            {
                Address = new AddressModel()
                {
                    City = address.City,
                    Country = (await _countryService.GetCountryByIdAsync(address.CountryId ?? 0))?.Name,
                    Line1 = address.Address1,
                    Line2 = address.Address2,
                    State = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name,
                    Zipcode = address.ZipPostalCode
                },
                Email = address.Email,
                Name = new NameModel()
                {
                    First = address.FirstName,
                    Last = address.LastName
                }
            };
        }

        #endregion
    }
}