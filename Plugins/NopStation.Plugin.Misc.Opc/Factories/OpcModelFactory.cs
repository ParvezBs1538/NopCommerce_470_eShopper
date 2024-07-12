using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.Misc.Opc.Models;
using AddressModel = Nop.Web.Models.Common.AddressModel;
using EstimateShippingModel = NopStation.Plugin.Misc.Opc.Models.EstimateShippingModel;
using EstimateShippingResultModel = NopStation.Plugin.Misc.Opc.Models.EstimateShippingResultModel;

namespace NopStation.Plugin.Misc.Opc.Factories;

public class OpcModelFactory : IOpcModelFactory
{
    #region Fields

    private readonly OpcSettings _opcSettings;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly OrderSettings _orderSettings;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ICustomerService _customerService;
    private readonly ShippingSettings _shippingSettings;
    private readonly ICountryService _countryService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IAddressService _addressService;
    private readonly AddressSettings _addressSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShippingService _shippingService;
    private readonly ILocalizationService _localizationService;
    private readonly ITaxService _taxService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICurrencyService _currencyService;
    private readonly ICheckoutModelFactory _checkoutModelFactory;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly PaymentSettings _paymentSettings;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IRewardPointService _rewardPointService;
    private readonly IPaymentService _paymentService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly RewardPointsSettings _rewardPointsSettings;
    private readonly IAddressModelFactory _addressModelFactory;

    #endregion Fields

    #region Ctor

    public OpcModelFactory(OpcSettings opcSettings,
        IWorkContext workContext,
        IStoreContext storeContext,
        OrderSettings orderSettings,
        IShoppingCartService shoppingCartService,
        ICustomerService customerService,
        ShippingSettings shippingSettings,
        ICountryService countryService,
        IStoreMappingService storeMappingService,
        IAddressService addressService,
        AddressSettings addressSettings,
        IHttpContextAccessor httpContextAccessor,
        IShippingService shippingService,
        ILocalizationService localizationService,
        ITaxService taxService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IStateProvinceService stateProvinceService,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        ICheckoutModelFactory checkoutModelFactory,
        IGenericAttributeService genericAttributeService,
        IOrderProcessingService orderProcessingService,
        PaymentSettings paymentSettings,
        IPaymentPluginManager paymentPluginManager,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IRewardPointService rewardPointService,
        IPaymentService paymentService,
        IDateTimeHelper dateTimeHelper,
        RewardPointsSettings rewardPointsSettings,
        IAddressModelFactory addressModelFactory)
    {
        _opcSettings = opcSettings;
        _workContext = workContext;
        _storeContext = storeContext;
        _orderSettings = orderSettings;
        _shoppingCartService = shoppingCartService;
        _customerService = customerService;
        _shippingSettings = shippingSettings;
        _countryService = countryService;
        _storeMappingService = storeMappingService;
        _addressService = addressService;
        _addressSettings = addressSettings;
        _httpContextAccessor = httpContextAccessor;
        _shippingService = shippingService;
        _localizationService = localizationService;
        _taxService = taxService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _stateProvinceService = stateProvinceService;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _checkoutModelFactory = checkoutModelFactory;
        _genericAttributeService = genericAttributeService;
        _orderProcessingService = orderProcessingService;
        _paymentSettings = paymentSettings;
        _paymentPluginManager = paymentPluginManager;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _rewardPointService = rewardPointService;
        _paymentService = paymentService;
        _dateTimeHelper = dateTimeHelper;
        _rewardPointsSettings = rewardPointsSettings;
        _addressModelFactory = addressModelFactory;
    }

    #endregion Ctor

    private async Task SetDefaultShippingOptionsAsync(CheckoutShippingMethodModel shippingMethodModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        if (await _genericAttributeService.GetAttributeAsync<ShippingOption>(currentCustomer,
            NopCustomerDefaults.SelectedShippingOptionAttribute, currentStore.Id) == null)
        {
            var model = shippingMethodModel.ShippingMethods.FirstOrDefault(a => a.Selected);
            if (model != null)
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, model.ShippingOption, currentStore.Id);
            }
        }
    }

    #region Methods

    public async Task<OpcModel> PrepareOpcModelAsync(IList<ShoppingCartItem> cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var model = new OpcModel
        {
            DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep && (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).Any(),
            BillingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, _opcSettings.DefaultBillingAddressCountryId, false),
            ConfirmOrder = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart),
            ShowEstimateShipping = _opcSettings.ShowEstimateShippingInCheckout
        };

        if (model.DisableBillingAddressCheckoutStep)
        {
            var firstAddress = (await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).FirstOrDefault(a => a.Id == model.BillingAddressModel.ExistingAddresses.First().Id);
            if (firstAddress != null)
            {
                customer.BillingAddressId = firstAddress.Id;
                await _customerService.UpdateCustomerAsync(customer);
                model.DisableBillingAddressCheckoutStep = true;
            }
            else
            {
                model.DisableBillingAddressCheckoutStep = false;
            }
        }

        if (!model.DisableBillingAddressCheckoutStep)
            if (_opcSettings.PreselectPreviousBillingAddress && (await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0)) != null)
            {
                model.BillingAddressModel.CustomProperties.Add("CustomerBillingAddress", customer.BillingAddressId?.ToString());

                var address = await _addressService.GetAddressByIdAsync((int)customer.BillingAddressId);

                var addressModel = new AddressModel();

                await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                        address: address,
                        excludeProperties: false,
                        addressSettings: _addressSettings,
                        loadCountries: async () => await _countryService.GetAllCountriesForBillingAsync((await _workContext.GetWorkingLanguageAsync()).Id));

                model.BillingAddressModel.BillingNewAddress = addressModel;
            }
            else
            {
                if (_httpContextAccessor.HttpContext.Session.TryGetValue(OpcDefaults.BillingAddressSessionKey, out AddressModel billingAddress) && billingAddress.Id == 0)
                {
                    model.BillingAddressModel.BillingNewAddress = billingAddress;
                }
                else
                {
                    await _httpContextAccessor.HttpContext.Session.SetAsync(OpcDefaults.BillingAddressSessionKey, model.BillingAddressModel.BillingNewAddress);
                }

                model.BillingAddressModel.NewAddressPreselected = true;
                customer.BillingAddressId = null;
                await _customerService.UpdateCustomerAsync(customer);
            }

        if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            model.ShippingRequired = true;
            model.ShippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, _opcSettings.DefaultShippingAddressCountryId, false);
            model.ShippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));

            if (_opcSettings.PreselectPreviousShippingAddress && (await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0)) != null)
            {
                model.ShippingAddressModel.CustomProperties.Add("CustomerShippingAddress", customer.ShippingAddressId?.ToString());
                var address = await _addressService.GetAddressByIdAsync((int)customer.ShippingAddressId);

                var addressModel = new AddressModel();
                await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                        address: address,
                        excludeProperties: false,
                        addressSettings: _addressSettings,
                        loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id));

                model.ShippingAddressModel.ShippingNewAddress = addressModel;
            }
            else
            {
                if (_httpContextAccessor.HttpContext.Session.TryGetValue(OpcDefaults.ShippingAddressSessionKey, out AddressModel shippingAddress) && shippingAddress.Id == 0)
                {
                    model.ShippingAddressModel.ShippingNewAddress = shippingAddress;
                }
                else
                {
                    _httpContextAccessor.HttpContext.Session.SetAsync(OpcDefaults.ShippingAddressSessionKey, model.ShippingAddressModel.ShippingNewAddress).GetAwaiter().GetResult();
                }

                model.ShippingAddressModel.NewAddressPreselected = true;
                customer.ShippingAddressId = null;
                await _customerService.UpdateCustomerAsync(customer);
            }

            //Check if PreselectShipToSameAddress
            if (!_orderSettings.DisableBillingAddressCheckoutStep && model.BillingAddressModel.ShipToSameAddressAllowed)
            {
                if (_opcSettings.PreselectShipToSameAddress)
                {
                    model.BillingAddressModel.ShipToSameAddress = true;

                    //save shipping addess as billing

                    customer.ShippingAddressId = customer.BillingAddressId;
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    model.BillingAddressModel.ShipToSameAddress = false;
                }
            }

            await SetDefaultShippingOptionsAsync(model.ShippingMethodModel);

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne && model.ShippingMethodModel.ShippingMethods.Count == 1)
            {
                model.BypassShippingMethodSelection = true;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    model.ShippingMethodModel.ShippingMethods.First().ShippingOption,
                    store.Id);
            }
        }

        if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false))
        {
            model.PaymentWorkflowRequired = true;

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ??
                    _opcSettings.DefaultBillingAddressCountryId;
            }

            //payment is required
            model.PaymentMethodModel = await PreparePaymentMethodsModelAsync(cart, filterByCountryId);

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                model.PaymentMethodModel.PaymentMethods.Count == 1 && !model.PaymentMethodModel.DisplayRewardPoints)
            {
                model.BypassPaymentMethodSelection = true;

                var selectedPaymentMethodSystemName = model.PaymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    selectedPaymentMethodSystemName, store.Id);

                var paymentMethodInst = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName, customer, store.Id);
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    throw new Exception("Selected payment method can't be parsed");
            }
        }

        if (_opcSettings.ShowShoppingCartInCheckout)
        {
            model.ShowShoppingCart = true;
            model.ShowCheckoutAttributes = _opcSettings.ShowCheckoutAttributesInCheckout;
            model.ShowDiscountBox = _opcSettings.ShowDiscountBoxInCheckout;
            model.ShowGiftCardBox = _opcSettings.ShowGiftCardBoxInCheckout;
            model.ShowOrderReviewData = _opcSettings.ShowOrderReviewDataInCheckout;
            model.ShoppingCartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart, isEditable: _opcSettings.IsShoppingCartEditable, prepareAndDisplayOrderReviewData: _opcSettings.ShowOrderReviewDataInCheckout);
            model.ShoppingCartModel.DiscountBox.Display = model.ShowDiscountBox;
            model.ShoppingCartModel.GiftCardBox.Display = model.ShowGiftCardBox;
        }

        return model;
    }

    public async Task<CheckoutShippingMethodModel> PrepareShippingMethodModelAsync(IList<ShoppingCartItem> cart, Address shippingAddress)
    {
        var checkoutShippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, shippingAddress);
        CheckoutShippingMethodModel.ShippingMethodModel shippingMethodModel = checkoutShippingMethodModel.ShippingMethods.FirstOrDefault((CheckoutShippingMethodModel.ShippingMethodModel option) => option.Selected);
        if (shippingMethodModel != null)
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, shippingMethodModel.ShippingOption, (await _storeContext.GetCurrentStoreAsync()).Id);
        }
        else
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedShippingOptionAttribute, null, (await _storeContext.GetCurrentStoreAsync()).Id);
        }
        return checkoutShippingMethodModel;
    }

    public virtual async Task<CheckoutPaymentMethodModel> PreparePaymentMethodsModelAsync(IList<ShoppingCartItem> cart, int countryId = 0)
    {
        var model = new CheckoutPaymentMethodModel();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        //reward points
        if (_rewardPointsSettings.Enabled && !await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
        {
            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, true, false);
            if (shoppingCartTotal.redeemedRewardPoints > 0)
            {
                model.DisplayRewardPoints = true;
                model.RewardPointsToUseAmount = await _priceFormatter.FormatPriceAsync(shoppingCartTotal.redeemedRewardPointsAmount, true, false);
                model.RewardPointsToUse = shoppingCartTotal.redeemedRewardPoints;
                model.RewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, store.Id);
                model.UseRewardPoints = (await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, (await _storeContext.GetCurrentStoreAsync()).Id));
                //are points enough to pay for entire order? like if this option (to use them) was selected
                model.RewardPointsEnoughToPayForOrder = !await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, true);
            }
        }
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, null);
        if (isPaymentWorkflowRequired)
        {
            //filter by country
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id, countryId))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
                .ToListAsync();
            foreach (var pm in paymentMethods)
            {
                if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
                {
                    Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id),
                    Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.GetPaymentMethodDescriptionAsync() : string.Empty,
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                };
                //payment method additional fee
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, pm.PluginDescriptor.SystemName);
                var (rateBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, await _workContext.GetCurrentCustomerAsync());
                var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, await _workContext.GetWorkingCurrencyAsync());
                if (rate > decimal.Zero)
                    pmModel.Fee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(rate, true);

                //Check whether payment info should be skipped
                if (pm.SkipPaymentInfo ||
                    (pm.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
                {
                    //skip payment info page
                    pmModel.CustomProperties.Add("PaymentInfo", null);
                }
                else
                {
                    var checkoutPaymentInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(pm);
                    var jsonString = JsonConvert.SerializeObject(checkoutPaymentInfoModel);
                    pmModel.CustomProperties.Add("PaymentInfo", jsonString);
                }

                model.PaymentMethods.Add(pmModel);
            }

            //find a selected (previously) payment method
            var selectedPaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!string.IsNullOrEmpty(selectedPaymentMethodSystemName))
            {
                var paymentMethodToSelect = model.PaymentMethods.ToList()
                    .Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }
            //if no option has been selected, let's do it for the first one
            if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                {
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        paymentMethodToSelect.PaymentMethodSystemName, store.Id);
                    paymentMethodToSelect.Selected = true;
                }
            }
            foreach (var paymentMethod in model.PaymentMethods)
            {
                if (paymentMethod.Selected == true)
                    continue;

                string val = null;
                if (paymentMethod.CustomProperties.TryGetValue("PaymentInfo", out val))
                {
                    // key exist
                    paymentMethod.CustomProperties["PaymentInfo"] = null;
                }
            }
        }

        //model.CustomProperties.Add("IsPaymentWorkFlowRequired", isPaymentWorkflowRequired);
        model.CustomProperties.Add("IsPaymentWorkFlowRequired", isPaymentWorkflowRequired.ToString());

        if (model.PaymentMethods.Count == 0)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPaymentMethodAttribute,
                   null, (await _storeContext.GetCurrentStoreAsync()).Id);
        }
        return model;
    }

    public virtual Task<CheckoutPaymentInfoModel> PreparePaymentInfoModelAsync(IPaymentMethod paymentMethod)
    {
        return Task.FromResult(new CheckoutPaymentInfoModel
        {
            PaymentViewComponent = paymentMethod.GetPublicViewComponent()
        });
    }

    public virtual async Task<EstimateShippingModel> PrepareEstimateShippingModelAsync(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var model = new EstimateShippingModel
        {
            RequestDelay = _shippingSettings.RequestDelay,
            UseCity = _shippingSettings.EstimateShippingCityNameEnabled,
            Enabled = cart.Any() && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart)
        };
        if (model.Enabled)
        {
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(await _workContext.GetCurrentCustomerAsync());
            if (shippingAddress == null)
            {
                shippingAddress = await (await _customerService.GetAddressesByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id))
                //enabled for the current store
                .FirstOrDefaultAwaitAsync(async a => a.CountryId == null || await _storeMappingService.AuthorizeAsync(await _countryService.GetCountryByAddressAsync(a)));
            }

            //countries
            var defaultEstimateCountryId = (setEstimateShippingDefaultAddress && shippingAddress != null)
                ? shippingAddress.CountryId
                : model.CountryId;
            model.AvailableCountries.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Address.SelectCountry"),
                Value = "0"
            });

            foreach (var c in await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id))
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == defaultEstimateCountryId
                });

            //states
            var defaultEstimateStateId = (setEstimateShippingDefaultAddress && shippingAddress != null)
                ? shippingAddress.StateProvinceId
                : model.StateProvinceId;
            var states = defaultEstimateCountryId.HasValue
                ? (await _stateProvinceService.GetStateProvincesByCountryIdAsync(defaultEstimateCountryId.Value, (await _workContext.GetWorkingLanguageAsync()).Id)).ToList()
                : new List<StateProvince>();
            if (states.Any())
            {
                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                        Value = s.Id.ToString(),
                        Selected = s.Id == defaultEstimateStateId
                    });
                }
            }
            else
            {
                model.AvailableStates.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Address.Other"),
                    Value = "0"
                });
            }

            if (setEstimateShippingDefaultAddress && shippingAddress != null)
            {
                if (!_shippingSettings.EstimateShippingCityNameEnabled)
                    model.ZipPostalCode = shippingAddress.ZipPostalCode;
                else
                    model.City = shippingAddress.City;
            }
        }

        return model;
    }

    public async Task<EstimateShippingResultModel> PrepareEstimateShippingResultModelAsync(IList<ShoppingCartItem> cart, EstimateShippingModel request, bool cacheShippingOptions)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var model = new EstimateShippingResultModel();

        if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            var address = new Address
            {
                CountryId = request.CountryId,
                StateProvinceId = request.StateProvinceId,
                ZipPostalCode = request.ZipPostalCode,
                City = request.City
            };

            var rawShippingOptions = new List<ShippingOption>();

            var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, address, await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
            if (getShippingOptionResponse.Success)
            {
                if (getShippingOptionResponse.ShippingOptions.Any())
                {
                    foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                    {
                        rawShippingOptions.Add(new ShippingOption
                        {
                            Name = shippingOption.Name,
                            Description = shippingOption.Description,
                            Rate = shippingOption.Rate,
                            TransitDays = shippingOption.TransitDays,
                            ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName
                        });
                    }
                }
                else
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.Errors.Add(error);
                }
            }

            var pickupPointsNumber = 0;
            if (_shippingSettings.AllowPickupInStore)
            {
                var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(cart, address, await _workContext.GetCurrentCustomerAsync(), storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                if (pickupPointsResponse.Success)
                {
                    if (pickupPointsResponse.PickupPoints.Any())
                    {
                        pickupPointsNumber = pickupPointsResponse.PickupPoints.Count();
                        var pickupPoint = pickupPointsResponse.PickupPoints.OrderBy(p => p.PickupFee).First();

                        rawShippingOptions.Add(new ShippingOption
                        {
                            Name = await _localizationService.GetResourceAsync("Checkout.PickupPoints"),
                            Description = await _localizationService.GetResourceAsync("Checkout.PickupPoints.Description"),
                            Rate = pickupPoint.PickupFee,
                            TransitDays = pickupPoint.TransitDays,
                            ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                            IsPickupInStore = true
                        });
                    }
                }
                else
                {
                    foreach (var error in pickupPointsResponse.Errors)
                        model.Errors.Add(error);
                }
            }

            ShippingOption selectedShippingOption = null;
            if (cacheShippingOptions)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                                                       NopCustomerDefaults.OfferedShippingOptionsAttribute,
                                                       rawShippingOptions,
                                                       (await _storeContext.GetCurrentStoreAsync()).Id);

                //find a selected (previously) shipping option
                selectedShippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                        NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            }

            if (rawShippingOptions.Any())
            {
                foreach (var option in rawShippingOptions)
                {
                    var (shippingRate, _) = await _orderTotalCalculationService.AdjustShippingRateAsync(option.Rate, cart, option.IsPickupInStore);
                    (shippingRate, _) = await _taxService.GetShippingPriceAsync(shippingRate, await _workContext.GetCurrentCustomerAsync());
                    shippingRate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shippingRate, await _workContext.GetWorkingCurrencyAsync());
                    var shippingRateString = await _priceFormatter.FormatShippingPriceAsync(shippingRate, true);

                    if (option.IsPickupInStore && pickupPointsNumber > 1)
                        shippingRateString = string.Format(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.Pickup.PriceFrom"), shippingRateString);

                    string deliveryDateFormat = null;
                    if (option.TransitDays.HasValue)
                    {
                        var currentCulture = CultureInfo.GetCultureInfo((await _workContext.GetWorkingLanguageAsync()).LanguageCulture);
                        var customerDateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                        deliveryDateFormat = customerDateTime.AddDays(option.TransitDays.Value).ToString("d", currentCulture);
                    }

                    var selected = selectedShippingOption != null &&
                                    !string.IsNullOrEmpty(option.ShippingRateComputationMethodSystemName) &&
                                    option.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase) &&
                                    (!string.IsNullOrEmpty(option.Name) &&
                                     option.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) ||
                                     (option.IsPickupInStore && option.IsPickupInStore == selectedShippingOption.IsPickupInStore));

                    model.ShippingOptions.Add(new EstimateShippingResultModel.ShippingOptionModel
                    {
                        Name = option.Name,
                        ShippingRateComputationMethodSystemName = option.ShippingRateComputationMethodSystemName,
                        Description = option.Description,
                        Price = shippingRateString,
                        Rate = option.Rate,
                        DeliveryDateFormat = deliveryDateFormat,
                        Selected = selected
                    });
                }

                //if no option has been selected, let's do it for the first one
                if (!model.ShippingOptions.Any(so => so.Selected))
                    model.ShippingOptions.First().Selected = true;
            }
        }

        return model;
    }

    #endregion Methods
}