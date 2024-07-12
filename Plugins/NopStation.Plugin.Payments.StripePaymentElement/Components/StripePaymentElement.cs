using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripePaymentElement.Models;
using NopStation.Plugin.Payments.StripePaymentElement.Services;

namespace NopStation.Plugin.Payments.StripePaymentElement.Components;

public class StripePaymentElementViewComponent : NopStationViewComponent
{
    #region Field

    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly AddressSettings _addressSettings;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ICurrencyService _currencyService;
    private readonly ICountryService _countryService;
    private readonly ICustomerService _customerService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IStoreContext _storeContext;
    private readonly StripePaymentElementSettings _stripePaymentElementSettings;

    #endregion

    #region Ctor

    public StripePaymentElementViewComponent(IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        AddressSettings addressSettings,
        IAddressModelFactory addressModelFactory,
        IOrderTotalCalculationService orderTotalCalculationService,
        ICurrencyService currencyService,
        ICountryService countryService,
        ICustomerService customerService,
        IPaymentPluginManager paymentPluginManager,
        IStoreContext storeContext,
        StripePaymentElementSettings stripePaymentElementSettings)
    {
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _addressSettings = addressSettings;
        _addressModelFactory = addressModelFactory;
        _orderTotalCalculationService = orderTotalCalculationService;
        _currencyService = currencyService;
        _countryService = countryService;
        _customerService = customerService;
        _paymentPluginManager = paymentPluginManager;
        _storeContext = storeContext;
        _stripePaymentElementSettings = stripePaymentElementSettings;
    }

    #endregion

    #region Methods 

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        if (!await _paymentPluginManager.IsPluginActiveAsync(StripeDefaults.SystemName, customer, store?.Id ?? 0))
            return Content(string.Empty);

        if (!StripePaymentElementManager.IsConfigured(_stripePaymentElementSettings))
            return Content(string.Empty);

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer);
        var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);
        var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(customer);
        var country = await _countryService.GetCountryByIdAsync(billingAddress.CountryId.Value);

        //total
        var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        var currrency = await _workContext.GetWorkingCurrencyAsync();
        var model = new PaymentInfoModel
        {
            Currency = currrency.CurrencyCode.ToLower(),
            Country = country.TwoLetterIsoCode.ToUpper(),
            PublishableKey = _stripePaymentElementSettings.PublishableKey,
            Theme = _stripePaymentElementSettings.Theme,
            Layout = _stripePaymentElementSettings.Layout
        };

        if (shoppingCartTotalBase.HasValue)
        {
            var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
            model.OrderTotal = Math.Floor(shoppingCartTotal * 100).ToString();
        }

        await _addressModelFactory.PrepareAddressModelAsync(
            model.BillingAddress,
            billingAddress,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: null,
            prePopulateWithCustomerFields: false,
            customer: customer);

        model.BillingAddress.CountryName = (await _countryService.GetCountryByIdAsync(billingAddress.CountryId ?? 0))?.TwoLetterIsoCode;

        if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            await _addressModelFactory.PrepareAddressModelAsync(
            model.ShippingAddress,
            shippingAddress,
            excludeProperties: false,
            addressSettings: _addressSettings,
            loadCountries: null,
            prePopulateWithCustomerFields: false,
            customer: customer);

            model.ShippingAddress.CountryName = (await _countryService.GetCountryByIdAsync(shippingAddress.CountryId ?? 0))?.TwoLetterIsoCode;
        }

        return View(model);
    }

    #endregion
}
