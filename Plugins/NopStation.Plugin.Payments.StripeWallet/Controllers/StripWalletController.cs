using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripeWallet.Models;
using Stripe;

namespace NopStation.Plugin.Payments.StripeWallet.Controllers
{
    public class StripWalletController : NopStationPublicController
    {
        #region Field  

        private readonly IStoreContext _storeContext;
        private readonly StripeWalletSettings _stripeWalletSettings;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public StripWalletController(IStoreContext storeContext,
            StripeWalletSettings stripeDigitalWalletSettings,
            INopFileProvider nopFileProvider,
            ILogger logger, IWorkContext workContext, IShoppingCartService shoppingCartService, IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService, ICountryService countryService, ICustomerService customerService)
        {
            _storeContext = storeContext;
            _stripeWalletSettings = stripeDigitalWalletSettings;
            _nopFileProvider = nopFileProvider;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer);

            //total
            var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            var currrency = await _workContext.GetWorkingCurrencyAsync();

            var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)Math.Floor(shoppingCartTotal * 100),
                Currency = currrency.CurrencyCode.ToLower(),
                PaymentMethodTypes = new List<string> { "card" },
                CaptureMethod = "manual"
            };

            var service = new PaymentIntentService(new StripeClient(apiKey: _stripeWalletSettings.SecretKey));

            try
            {
                var paymentIntent = await service.CreateAsync(options);

                return Json(new CreatePaymentIntentResponse
                {
                    Result = "success",
                    ClientSecret = paymentIntent.ClientSecret,
                });
            }
            catch (StripeException e)
            {
                return Json(new { result = "fail", error = new { message = e.StripeError.Message } });
            }
            catch (Exception)
            {
                return Json(new { result = "fail", error = new { message = "unknown failure: 500" } });
            }
        }

        [Route(".well-known/apple-developer-merchantid-domain-association")]
        [HttpGet]
        public async Task<IActionResult> RenderMerchantId()
        {
            var storeId = _storeContext.GetCurrentStore().Id;
            var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeId));

            if (!_nopFileProvider.FileExists(filePath))
                filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, 0));

            if (!_nopFileProvider.FileExists(filePath))
                return InvokeHttp404();

            return Content(await System.IO.File.ReadAllTextAsync(filePath), "text/plain");

        }

        #endregion
    }
}
