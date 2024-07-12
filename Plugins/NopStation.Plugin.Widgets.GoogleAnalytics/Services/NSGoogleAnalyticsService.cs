using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Widgets.GoogleAnalytics.Models;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Services
{
    public class NSGoogleAnalyticsService : INSGoogleAnalyticsService
    {
        #region Fields

        private readonly NopstationGoogleAnalyticsSettings _googleAnalyticsSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreContext _storeContext;
        private readonly OrderSettings _orderSettings;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly ICurrencyService _currencyService;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public NSGoogleAnalyticsService(CatalogSettings catalogSettings,
            ICurrencyService currencyService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ITaxService taxService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            NopstationGoogleAnalyticsSettings googleAnalyticsSettings,
            IHttpClientFactory httpClientFactory,
            ILogger logger,
            IHttpContextAccessor httpContextAccessor,
            IStoreContext storeContext)
        {
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _taxService = taxService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _googleAnalyticsSettings = googleAnalyticsSettings;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _storeContext = storeContext;
            _logger = logger;
        }

        #endregion

        #region Utilities

        private string GetClientIdFromCookie()
        {
            string cookie_val = _httpContextAccessor.HttpContext?.Request?.Cookies["nopstation_ga"];
            string clientId = string.Empty;
            if (cookie_val != null)
            {
                var split_cookie_val = cookie_val.Split('.');
                var first = split_cookie_val[split_cookie_val.Length - 2];
                var second = split_cookie_val[split_cookie_val.Length - 1];
                clientId = first + "." + second;
            }
            return clientId;
        }

        #endregion

        #region Methods

        public async Task<string> PostAsync<T>(T eventParam, string eventName)
        {
            var measurement_id = _googleAnalyticsSettings.GoogleId;
            var api_secret = _googleAnalyticsSettings.ApiSecret;
            var client_id = GetClientIdFromCookie();

            var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurement_id}&api_secret={api_secret}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postData = new GoogleAnalyticsRequest()
            {
                client_id = client_id,
                events = new List<Event>()
                {
                    new Event
                    {
                        name = eventName,
                        @params = eventParam
                    }
                }
            };

            var content = JsonConvert.SerializeObject(postData);
            request.Content = new StringContent(content);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var client = _httpClientFactory.CreateClient();

            try
            {
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = JsonConvert.SerializeObject(response);

                    if (_googleAnalyticsSettings.SaveLog)
                        await _logger.InformationAsync(string.Format("URL: {0}, Body: {1}", url, JsonConvert.SerializeObject(postData)));

                    return responseString;
                }
                if (_googleAnalyticsSettings.SaveLog)
                    await _logger.ErrorAsync(string.Format("Status code: {0}, URL: {1}, Body: {2}", response.StatusCode, url, JsonConvert.SerializeObject(postData)));
            }
            catch (Exception ex)
            {
                if (_googleAnalyticsSettings.SaveLog)
                    await _logger.ErrorAsync(string.Format("Exception: {0}, URL: {1}, Body: {2}", ex.Message, url, JsonConvert.SerializeObject(postData)));
            }

            return "";
        }

        public virtual async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                        var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store, includeDiscounts: false)).finalPrice);
                        var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store)).finalPrice);
                        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                        var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                        var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                            model.OldPriceValue = oldPrice;
                        }

                        model.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                        {
                            model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.PriceWithDiscountValue = finalPriceWithDiscount;
                        }

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                        model.BasePricePAngVValue = finalPriceWithDiscountBase;
                        //currency code
                        model.CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                            model.RentalPriceValue = finalPriceWithDiscount;
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.OldPriceValue = null;
                model.Price = null;
            }

            return model;
        }

        #endregion
    }
}