using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeWallet.Models;

namespace NopStation.Plugin.Payments.StripeWallet.Components
{
    public class StripeWalletViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly StripeWalletSettings _stripeWalletSettings;

        #endregion

        #region Ctor

        public StripeWalletViewComponent(IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            ICountryService countryService,
            ICustomerService customerService,
            StripeWalletSettings stripeWalletSettings)
        {
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _countryService = countryService;
            _customerService = customerService;
            _stripeWalletSettings = stripeWalletSettings;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer);
            var address = await _customerService.GetCustomerBillingAddressAsync(customer);
            var country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);

            //total
            var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            var currrency = await _workContext.GetWorkingCurrencyAsync();
            var model = new PaymentInfoModel
            {
                Currency = currrency.CurrencyCode.ToLower(),
                Country = country.TwoLetterIsoCode.ToUpper(),
                PublishableKey = _stripeWalletSettings.PublishableKey
            };

            if (shoppingCartTotalBase.HasValue)
            {
                var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
                model.OrderTotal = Math.Floor(shoppingCartTotal * 100).ToString();
            }

            return View(model);
        }

        #endregion
    }
}
