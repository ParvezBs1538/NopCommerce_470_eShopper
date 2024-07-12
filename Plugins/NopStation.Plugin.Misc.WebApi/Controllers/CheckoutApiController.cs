using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Models.Checkout;

namespace NopStation.Plugin.Misc.WebApi.Controllers
{
    [Route("api/checkout")]
    public partial class CheckoutApiController : BaseApiController
    {
        #region Fields

        protected readonly AddressSettings _addressSettings;
        protected readonly CaptchaSettings _captchaSettings;
        protected readonly CustomerSettings _customerSettings;
        protected readonly IAddressModelFactory _addressModelFactory;
        protected readonly IAddressService _addressService;
        protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _attributeService;
        protected readonly ICheckoutModelFactory _checkoutModelFactory;
        protected readonly ICountryService _countryService;
        protected readonly ICustomerService _customerService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly ILocalizationService _localizationService;
        protected readonly ILogger _logger;
        protected readonly IOrderProcessingService _orderProcessingService;
        protected readonly IOrderService _orderService;
        protected readonly IPaymentPluginManager _paymentPluginManager;
        protected readonly IPaymentService _paymentService;
        protected readonly IProductService _productService;
        protected readonly IShippingService _shippingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        protected readonly IShoppingCartService _shoppingCartService;
        protected readonly IStoreContext _storeContext;
        protected readonly ITaxService _taxService;
        protected readonly IWebHelper _webHelper;
        protected readonly IWorkContext _workContext;
        protected readonly OrderSettings _orderSettings;
        protected readonly PaymentSettings _paymentSettings;
        protected readonly RewardPointsSettings _rewardPointsSettings;
        protected readonly ShippingSettings _shippingSettings;
        protected readonly TaxSettings _taxSettings;
        private static readonly string[] _separator = ["___"];

        #endregion

        #region Ctor

        public CheckoutApiController(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeService<AddressAttribute, AddressAttributeValue> attributeService,
            ICheckoutModelFactory checkoutModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWebHelper webHelper,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _addressAttributeParser = addressAttributeParser;
            _attributeService = attributeService;
            _checkoutModelFactory = checkoutModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _taxService = taxService;
            _webHelper = webHelper;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task GenerateOrderGuidAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = await _genericAttributeService.GetPaymentRequestAttributeAsync(await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

            if (_paymentSettings.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest != null &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }

        protected async Task<string> ParseCustomAddressAttributes(NameValueCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            var attributesXml = string.Empty;

            foreach (var attribute in await _attributeService.GetAllAttributesAsync())
            {
                var controlId = string.Format(NopCommonDefaults.AddressAttributeControlName, attribute.Id);
                var attributeValues = form[controlId];
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                        if (!StringValues.IsNullOrEmpty(attributeValues) && int.TryParse(attributeValues, out var value) && value > 0)
                            attributesXml = _addressAttributeParser.AddAttribute(attributesXml, attribute, value.ToString());
                        break;

                    case AttributeControlType.Checkboxes:
                        foreach (var attributeValue in attributeValues.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (int.TryParse(attributeValue, out value) && value > 0)
                                attributesXml = _addressAttributeParser.AddAttribute(attributesXml, attribute, value.ToString());
                        }

                        break;

                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var addressAttributeValues = await _attributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var addressAttributeValue in addressAttributeValues)
                        {
                            if (addressAttributeValue.IsPreSelected)
                                attributesXml = _addressAttributeParser.AddAttribute(attributesXml, attribute, addressAttributeValue.Id.ToString());
                        }

                        break;

                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        if (!StringValues.IsNullOrEmpty(attributeValues))
                            attributesXml = _addressAttributeParser.AddAttribute(attributesXml, attribute, attributeValues.ToString().Trim());
                        break;

                    case AttributeControlType.Datepicker:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                    case AttributeControlType.FileUpload:
                    default:
                        break;
                }
            }

            return attributesXml;
        }

        protected virtual async Task<IActionResult> LoadStepAfterShippingAddress(IList<ShoppingCartItem> cart, GenericResponseModel<OpcStepResponseModel> response)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart,
                await _customerService.GetCustomerShippingAddressAsync(customer));

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    (await _storeContext.GetCurrentStoreAsync()).Id);

                //load next step
                return await LoadStepAfterShippingMethod(cart, response);
            }

            response.Data = new OpcStepResponseModel();
            response.Data.NextStep = OpcStep.ShippingMethod;
            response.Data.ShippingMethodModel = shippingMethodModel;

            return Ok(response);
        }

        protected virtual async Task<IActionResult> LoadStepAfterShippingMethod(IList<ShoppingCartItem> cart, GenericResponseModel<OpcStepResponseModel> response)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                }

                //payment is required
                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        selectedPaymentMethodSystemName, store.Id);

                    var paymentMethodInst = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName, customer, store.Id);
                    if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                        throw new Exception("Selected payment method can't be parsed");

                    return await LoadStepAfterPaymentMethod(paymentMethodInst, cart, response);
                }

                response.Data = new OpcStepResponseModel();
                response.Data.NextStep = OpcStep.PaymentMethod;
                response.Data.PaymentMethodModel = paymentMethodModel;

                return Ok(response);
            }

            //payment is not required
            await _genericAttributeService.SaveAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

            response.Data = new OpcStepResponseModel();
            response.Data.NextStep = OpcStep.ConfirmOrder;
            response.Data.ConfirmModel = await PrepareCheckoutConfirmOrderModel(cart);

            return Ok(response);
        }

        protected virtual async Task<IActionResult> LoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, IList<ShoppingCartItem> cart, GenericResponseModel<OpcStepResponseModel> response)
        {
            response.Data = new OpcStepResponseModel();

            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                await _genericAttributeService.SavePaymentRequestAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                    paymentInfo, (await _storeContext.GetCurrentStoreAsync()).Id);

                response.Data.NextStep = OpcStep.ConfirmOrder;
                response.Data.ConfirmModel = await PrepareCheckoutConfirmOrderModel(cart);

                return Ok(response);
            }

            //return payment info page
            var paymentInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);

            response.Data.NextStep = OpcStep.PaymentInfo;
            response.Data.PaymentInfoModel = paymentInfoModel;

            return Ok(response);
        }

        protected virtual async Task<CheckoutConfirmOrderModel> PrepareCheckoutConfirmOrderModel(IList<ShoppingCartItem> cart)
        {
            var cartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart, false, false, true);
            var processPaymentRequest = await _genericAttributeService.GetPaymentRequestAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
               (await _storeContext.GetCurrentStoreAsync()).Id);

            if (processPaymentRequest != null)
                cartModel.OrderReviewData.CustomValues = processPaymentRequest.CustomValues;

            return new CheckoutConfirmOrderModel()
            {
                Cart= cartModel,
                Confirm = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart),
                OrderTotals = await _shoppingCartModelFactory.PrepareOrderTotalsModelAsync(cart, false),
                SelectedCheckoutAttributes = await _shoppingCartModelFactory.FormatSelectedCheckoutAttributesAsync(),
                EstimateShipping = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(cart)
            };
        }

        protected virtual bool ParsePickupInStore(NameValueCollection form)
        {
            var pickupInStore = false;

            var pickupInStoreParameter = form["PickupInStore"];
            if (!StringValues.IsNullOrEmpty(pickupInStoreParameter))
                bool.TryParse(pickupInStoreParameter, out pickupInStore);

            return pickupInStore;
        }

        protected virtual async Task<PickupPoint> ParsePickupOptionAsync(IList<ShoppingCartItem> cart, NameValueCollection form)
        {
            var pickupPoint = form["pickup-points-id"].ToString().Split(new[] { "___" }, StringSplitOptions.None);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var address = customer.BillingAddressId.HasValue
                ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
                : null;

            var selectedPoint = (await _shippingService.GetPickupPointsAsync(cart, address,
                customer, pickupPoint[1], store.Id)).PickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

            if (selectedPoint == null)
                throw new Exception("Pickup point is not allowed");

            return selectedPoint;
        }

        protected virtual async Task SavePickupOptionAsync(PickupPoint pickupPoint)
        {
            var name = !string.IsNullOrEmpty(pickupPoint.Name) ?
                string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), pickupPoint.Name) :
                await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
            var pickUpInStoreShippingOption = new ShippingOption
            {
                Name = name,
                Rate = pickupPoint.PickupFee,
                Description = pickupPoint.Description,
                ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                IsPickupInStore = true
            };

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, pickupPoint, store.Id);
        }

        protected virtual async Task<string> SaveCustomerVatNumberAsync(string fullVatNumber, Customer customer)
        {
            var (vatNumberStatus, _, _) = await _taxService.GetVatNumberStatusAsync(fullVatNumber);
            customer.VatNumberStatus = vatNumberStatus;
            customer.VatNumber = fullVatNumber;
            await _customerService.UpdateCustomerAsync(customer);

            if (vatNumberStatus != VatNumberStatus.Valid && !string.IsNullOrEmpty(fullVatNumber))
            {
                var warning = await _localizationService.GetResourceAsync("Checkout.VatNumber.Warning");
                return string.Format(warning, await _localizationService.GetLocalizedEnumAsync(vatNumberStatus));
            }

            return string.Empty;
        }

        protected async Task<IActionResult> EditAddressAsync(AddressModel addressModel, NameValueCollection form, Func<Customer, IList<ShoppingCartItem>, Address, Task<IActionResult>> getResult)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                //find address (ensure that it belongs to the current customer)
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressModel.Id);
                if (address == null)
                    throw new Exception("Address can't be loaded");

                //custom address attributes
                var customAttributes = await ParseCustomAddressAttributes(form);
                var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);

                if (customAttributeWarnings.Any())
                    return BadRequest(errors: customAttributeWarnings);

                address = addressModel.ToEntity(address);
                address.CustomAttributes = customAttributes;

                await _addressService.UpdateAddressAsync(address);

                return await getResult(customer, cart, address);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return BadRequest(exc.Message);
            }
        }

        protected async Task<IActionResult> DeleteAddressAsync(int addressId, Func<IList<ShoppingCartItem>, Task<IActionResult>> getResult)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
                if (address != null)
                {
                    await _customerService.RemoveCustomerAddressAsync(customer, address);
                    await _customerService.UpdateCustomerAsync(customer);
                    await _addressService.DeleteAddressAsync(address);
                }

                return await getResult(cart);
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return BadRequest(exc.Message);
            }
        }

        #endregion

        #region Methods

        [HttpGet("getbilling")]
        public virtual async Task<IActionResult> GetBilling()
        {
            if (_orderSettings.CheckoutDisabled)
                return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

            var customer = await _workContext.GetCurrentCustomerAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

            var model = await _checkoutModelFactory.PrepareOnePageCheckoutModelAsync(cart);

            return OkWrap(model);
        }

        [HttpPost("savebilling")]
        public virtual async Task<IActionResult> SaveBilling([FromBody] BaseQueryModel<CheckoutBillingAddressModel> queryModel)
        {
            try
            {
                if (_orderSettings.CheckoutDisabled)
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

                var response = new GenericResponseModel<OpcStepResponseModel>
                {
                    Data = new OpcStepResponseModel()
                };

                var form = queryModel.FormValues.ToNameValueCollection();
                int.TryParse(form["billing_address_id"], out var billingAddressId);

                if (billingAddressId > 0)
                {
                    //existing address
                    var address = await _customerService.GetCustomerAddressAsync(customer.Id, billingAddressId);
                    if (address == null)
                        return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AddressCantBeLoaded"));

                    customer.BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    if (await _customerService.IsGuestAsync(customer) && _taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
                    {
                        var warning = await SaveCustomerVatNumberAsync(queryModel.Data.VatNumber, customer);
                        if (!string.IsNullOrEmpty(warning))
                            ModelState.AddModelError("", warning);
                    }

                    //new address
                    var newAddress = queryModel.Data.BillingNewAddress;

                    //custom address attributes
                    var customAttributes = await ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;

                        response.Data.NextStep = OpcStep.BillingAddress;
                        response.Data.BillingAddressModel = billingAddressModel;

                        foreach (var modelState in ModelState.Values)
                            foreach (var error in modelState.Errors)
                                response.ErrorList.Add(error.ErrorMessage);

                        return BadRequest(response);
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        //address is not found. let's create a new one
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;
                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        await _addressService.InsertAddressAsync(address);
                        await _customerService.InsertCustomerAddressAsync(customer, address);

                    }
                    customer.BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    //shipping is required
                    var address = await _customerService.GetCustomerBillingAddressAsync(customer);

                    //by default Shipping is available if the country is not specified
                    var shippingAllowed = _addressSettings.CountryEnabled ? (await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false : true;
                    if (_shippingSettings.ShipToSameAddress && queryModel.Data.ShipToSameAddress && shippingAllowed)
                    {
                        //ship to the same address
                        customer.ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                        //reset selected shipping method (in case if "pick up in store" was selected)
                        await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, (store).Id);
                        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, (store).Id);
                        //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 
                        return await LoadStepAfterShippingAddress(cart, response);
                    }

                    //do not ship to the same address
                    var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);

                    response.Data.NextStep = OpcStep.ShippingAddress;
                    response.Data.ShippingAddressModel = shippingAddressModel;

                    return Ok(response);
                }

                //shipping is not required
                customer.ShippingAddressId = null;
                await _customerService.UpdateCustomerAsync(customer);

                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, (store).Id);

                //load next step
                return await LoadStepAfterShippingMethod(cart, response);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.SaveBillingFailed"));
            }
        }

        [HttpPost("editbilling")]
        public virtual async Task<IActionResult> EditBillingAddress([FromBody] BaseQueryModel<CheckoutBillingAddressModel> queryModel)
        {
            try
            {
                var model = queryModel.Data;
                var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();
                return await EditAddressAsync(model.BillingNewAddress, form, async (customer, cart, address) =>
                {
                    customer.BillingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);

                    var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, address.CountryId);
                    return OkWrap(billingAddressModel);
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(exc.Message);
            }
        }

        [HttpPost("deletebilling/{addressId}")]
        public virtual async Task<IActionResult> DeleteBillingAddress(int addressId)
        {
            return await DeleteAddressAsync(addressId, async (cart) =>
            {
                var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart);
                return OkWrap(billingAddressModel);
            });
        }

        [HttpPost("saveshipping")]
        public virtual async Task<IActionResult> SaveShipping([FromBody] BaseQueryModel<CheckoutShippingAddressModel> queryModel)
        {
            try
            {
                if (_orderSettings.CheckoutDisabled)
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ShippingNotRequired"));

                var response = new GenericResponseModel<OpcStepResponseModel>
                {
                    Data = new OpcStepResponseModel()
                };
                var model = queryModel.Data;

                var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

                //pickup point
                if (_shippingSettings.AllowPickupInStore)
                {
                    var pickupInStore = ParsePickupInStore(form);

                    if (pickupInStore)
                    {
                        //no shipping address selected
                        customer.ShippingAddressId = null;
                        await _customerService.UpdateCustomerAsync(customer);

                        var pickupPoint = form["pickup-points-id"].ToString().Split(new[] { "___" }, StringSplitOptions.None);

                        var address = customer.BillingAddressId.HasValue
                            ? await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value)
                            : null;

                        var selectedPoint = (await _shippingService.GetPickupPointsAsync(cart, address,
                            customer, pickupPoint[1], store.Id)).PickupPoints.FirstOrDefault(x => x.Id.Equals(pickupPoint[0]));

                        if (selectedPoint == null)
                            throw new Exception("Pickup point is not allowed");

                        var name = !string.IsNullOrEmpty(selectedPoint.Name) ?
                            string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), selectedPoint.Name) :
                            await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
                        var pickUpInStoreShippingOption = new ShippingOption
                        {
                            Name = name,
                            Rate = selectedPoint.PickupFee,
                            Description = selectedPoint.Description,
                            ShippingRateComputationMethodSystemName = selectedPoint.ProviderSystemName,
                            IsPickupInStore = true
                        };
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedPickupPointAttribute, selectedPoint, store.Id);

                        //load next step
                        return await LoadStepAfterShippingMethod(cart, response);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                }

                int.TryParse(form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    //existing address
                    var address = await _customerService.GetCustomerAddressAsync(customer.Id, shippingAddressId)
                        ?? throw new Exception(await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                    customer.ShippingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    //new address
                    var newAddress = model.ShippingNewAddress;

                    //custom address attributes
                    var customAttributes = await ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    //validate model
                    if (!ModelState.IsValid)
                    {
                        //model is not valid. redisplay the form with errors
                        var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);

                        shippingAddressModel.NewAddressPreselected = true;

                        response.Data.NextStep = OpcStep.ShippingAddress;
                        response.Data.ShippingAddressModel = shippingAddressModel;

                        foreach (var modelState in ModelState.Values)
                            foreach (var error in modelState.Errors)
                                response.ErrorList.Add(error.ErrorMessage);

                        return BadRequest(response);
                    }

                    //try to find an address with the same values (don't duplicate records)
                    var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        await _addressService.InsertAddressAsync(address);

                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }

                    customer.ShippingAddressId = address.Id;

                    await _customerService.UpdateCustomerAsync(customer);
                }

                return await LoadStepAfterShippingAddress(cart, response);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.SaveShippingFailed"));
            }
        }

        [HttpPost("editshipping")]
        public virtual async Task<IActionResult> SaveEditShippingAddress([FromBody] BaseQueryModel<CheckoutShippingAddressModel> queryModel)
        {
            var model = queryModel.Data;
            var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

            return await EditAddressAsync(model.ShippingNewAddress, form, async (customer, cart, address) =>
            {
                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, address.CountryId);
                return OkWrap(shippingAddressModel);
            });
        }

        [HttpPost("deleteshipping/{addressId}")]
        public virtual async Task<IActionResult> DeleteEditShippingAddress(int addressId)
        {
            return await DeleteAddressAsync(addressId, async (cart) =>
            {
                var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart);
                return OkWrap(shippingAddressModel);
            });
        }

        [HttpPost("saveshippingmethod")]
        public virtual async Task<IActionResult> SaveShippingMethod([FromBody] BaseQueryModel<string> queryModel)
        {
            try
            {
                if (_orderSettings.CheckoutDisabled)
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var form = queryModel.FormValues.ToNameValueCollection();
                var shippingoption = form["shippingoption"];

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ShippingNotRequired"));

                var response = new GenericResponseModel<OpcStepResponseModel>
                {
                    Data = new OpcStepResponseModel()
                };

                //pickup point
                if (_shippingSettings.AllowPickupInStore && _orderSettings.DisplayPickupInStoreOnShippingMethodPage)
                {
                    var pickupInStore = ParsePickupInStore(form);
                    if (pickupInStore)
                    {
                        var pickupOption = await ParsePickupOptionAsync(cart, form);
                        await SavePickupOptionAsync(pickupOption);

                        return await LoadStepAfterShippingMethod(cart, response);
                    }

                    //set value indicating that "pick up in store" option has not been chosen
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                }

                //parse selected method 
                if (string.IsNullOrEmpty(shippingoption))
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ShippingCannotBeParsed"));
                var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ShippingCannotBeParsed"));
                var selectedName = splittedOption[0];
                var shippingRateComputationMethodSystemName = splittedOption[1];

                //find it
                //performance optimization. try cache first
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer),
                        customer, shippingRateComputationMethodSystemName, store.Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ShippingCannotBeLoaded"));

                //save
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, store.Id);

                //load next step
                return await LoadStepAfterShippingMethod(cart, response);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.SaveShippingMethodFailed"));
            }
        }

        [HttpPost("savepaymentmethod")]
        public virtual async Task<IActionResult> SavePaymentMethod([FromBody] BaseQueryModel<CheckoutPaymentMethodModel> queryModel)
        {
            try
            {
                if (_orderSettings.CheckoutDisabled)
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var form = queryModel.FormValues == null ? new NameValueCollection() : queryModel.FormValues.ToNameValueCollection();

                var paymentmethod = form["paymentmethod"];

                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

                //payment method 
                if (string.IsNullOrEmpty(paymentmethod))
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.PaymentCannotBeParsed"));

                var response = new GenericResponseModel<OpcStepResponseModel>();
                response.Data = new OpcStepResponseModel();
                var model = queryModel.Data;

                //reward points
                if (_rewardPointsSettings.Enabled)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints,
                        store.Id);
                }

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
                if (!isPaymentWorkflowRequired)
                {
                    //payment is not required
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

                    response.Data.NextStep = OpcStep.ConfirmOrder;
                    response.Data.ConfirmModel = await PrepareCheckoutConfirmOrderModel(cart);

                    return Ok(response);
                }

                var paymentMethodInst = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentmethod, customer, store.Id);
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.PaymentMethodNotFound"));

                //save
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentmethod, store.Id);

                return await LoadStepAfterPaymentMethod(paymentMethodInst, cart, response);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.SavePaymentMethodFailed"));
            }
        }

        [HttpGet("confirmorder")]
        public virtual async Task<IActionResult> GetConfirmOrder()
        {
            if (_orderSettings.CheckoutDisabled)
                return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            if (!cart.Any())
                return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

            //prevent 2 orders being placed within an X seconds time frame
            if (!await IsMinimumOrderPlacementIntervalValid(await _workContext.GetCurrentCustomerAsync()))
                return BadRequest(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

            var response = new GenericResponseModel<CheckoutConfirmOrderModel>();
            response.Data = await PrepareCheckoutConfirmOrderModel(cart);

            return Ok(response);
        }

        [HttpPost("confirmorder")]
        public virtual async Task<IActionResult> ConfirmOrder(bool captchaValid)
        {
            try
            {
                var customer = await _workContext.GetCurrentCustomerAsync();

                if (_orderSettings.CheckoutDisabled)
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (!cart.Any())
                    return BadRequest(await _localizationService.GetResourceAsync("ShoppingCart.CartIsEmpty"));

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValid(customer))
                    return BadRequest(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                var response = new GenericResponseModel<OpcStepResponseModel>();
                response.Data = new OpcStepResponseModel();

                //place order
                var processPaymentRequest = await _genericAttributeService.GetPaymentRequestAttributeAsync(customer, store.Id);

                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                await GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = (customer).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);

                await _genericAttributeService.SavePaymentRequestAttributeAsync(customer, processPaymentRequest, store.Id);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, NopStationCustomerDefaults.OrderPaymentInfo, null, store.Id);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync
                        (placeOrderResult.PlacedOrder.PaymentMethodSystemName, customer, store.Id);
                    if (paymentMethod == null)
                    {
                        response.Data.NextStep = OpcStep.Completed;
                        return Ok(response);
                    }

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        response.Data.NextStep = OpcStep.RedirectToGateway;
                        return Ok(response);
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    response.Data.NextStep = OpcStep.Completed;
                    return Ok(response);
                }

                //error
                foreach (var error in placeOrderResult.Errors)
                    response.ErrorList.Add(error);

                response.Data.NextStep = OpcStep.ConfirmOrder;

                return Ok(response);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return InternalServerError(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.ConfirmOrderFailed"));
            }
        }

        [HttpGet("completed/{orderId?}")]
        public virtual async Task<IActionResult> Completed(int? orderId)
        {
            //validation
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Unauthorized(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed"));

            Order order = null;
            if (orderId.HasValue)
            {
                //load order by identifier (if provided)
                order = await _orderService.GetOrderByIdAsync(orderId.Value);
            }
            if (order == null)
            {
                order = (await _orderService.SearchOrdersAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                    customerId: customer.Id, pageSize: 1)).FirstOrDefault();
            }
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
            {
                return BadRequest(await _localizationService.GetResourceAsync("NopStation.WebApi.Response.Checkout.OrderNotFound"));
            }

            var response = new GenericResponseModel<OpcStepResponseModel>();
            response.Data = new OpcStepResponseModel();

            //model
            var model = await _checkoutModelFactory.PrepareCheckoutCompletedModelAsync(order);
            response.Data.CompletedModel = model;

            return Ok(response);
        }

        #endregion
    }
}
