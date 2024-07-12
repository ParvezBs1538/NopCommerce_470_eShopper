using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Components;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Validators.Common;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Opc.Components;
using NopStation.Plugin.Misc.Opc.Factories;
using NopStation.Plugin.Misc.Opc.Models;
using EstimateShippingModel = NopStation.Plugin.Misc.Opc.Models.EstimateShippingModel;

namespace NopStation.Plugin.Misc.Opc.Controllers;

public class OpcController : NopStationPublicController
{
    #region Fields

    private readonly OrderSettings _orderSettings;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IOpcModelFactory _opcModelFactory;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ICustomerService _customerService;
    private readonly OpcSettings _opcSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IAddressService _addressService;
    private readonly IShippingService _shippingService;
    private readonly ILocalizationService _localizationService;
    private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly AddressSettings _addressSettings;
    private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly IDownloadService _downloadService;
    private readonly IOrderService _orderService;
    private readonly ShippingSettings _shippingSettings;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly PaymentSettings _paymentSettings;
    private readonly RewardPointsSettings _rewardPointsSettings;
    private readonly IGiftCardService _giftCardService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly ICountryService _countryService;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IPaymentService _paymentService;
    private readonly IWebHelper _webHelper;
    private readonly ICheckoutModelFactory _checkoutModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly ITaxService _taxService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICurrencyService _currencyService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly INopUrlHelper _nopUrlHelper;
    protected readonly IStaticCacheManager _staticCacheManager;
    private static readonly object _addressLocker = new object();

    #endregion Fields

    #region Ctor

    public OpcController(OrderSettings orderSettings,
        IStoreContext storeContext,
        IWorkContext workContext,
        IOpcModelFactory opcModelFactory,
        IShoppingCartService shoppingCartService,
        ICustomerService customerService,
        OpcSettings opcSettings,
        CustomerSettings customerSettings,
        IProductService productService,
        IDiscountService discountService,
        IGenericAttributeService genericAttributeService,
        IAddressService addressService,
        IShippingService shippingService,
        ILocalizationService localizationService,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        IStateProvinceService stateProvinceService,
        AddressSettings addressSettings,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IDownloadService downloadService,
        IOrderService orderService,
        ShippingSettings shippingSettings,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IOrderProcessingService orderProcessingService,
        IPaymentPluginManager paymentPluginManager,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        IGiftCardService giftCardService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IAddressModelFactory addressModelFactory,
        ICountryService countryService,
        IStoreMappingService storeMappingService,
        IPaymentService paymentService,
        IWebHelper webHelper,
        ICheckoutModelFactory checkoutModelFactory,
        IPermissionService permissionService,
        ITaxService taxService,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        IUrlRecordService urlRecordService,
        IProductAttributeService productAttributeService,
        IProductAttributeParser productAttributeParser,
        ShoppingCartSettings shoppingCartSettings,
        ICustomerActivityService customerActivityService,
        INopUrlHelper nopUrlHelper,
        IStaticCacheManager staticCacheManager)
    {
        _orderSettings = orderSettings;
        _storeContext = storeContext;
        _workContext = workContext;
        _opcModelFactory = opcModelFactory;
        _shoppingCartService = shoppingCartService;
        _customerService = customerService;
        _opcSettings = opcSettings;
        _customerSettings = customerSettings;
        _productService = productService;
        _discountService = discountService;
        _genericAttributeService = genericAttributeService;
        _addressService = addressService;
        _shippingService = shippingService;
        _localizationService = localizationService;
        _addressAttributeService = addressAttributeService;
        _stateProvinceService = stateProvinceService;
        _addressSettings = addressSettings;
        _addressAttributeParser = addressAttributeParser;
        _checkoutAttributeService = checkoutAttributeService;
        _checkoutAttributeParser = checkoutAttributeParser;
        _downloadService = downloadService;
        _orderService = orderService;
        _shippingSettings = shippingSettings;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _orderProcessingService = orderProcessingService;
        _paymentPluginManager = paymentPluginManager;
        _paymentSettings = paymentSettings;
        _rewardPointsSettings = rewardPointsSettings;
        _giftCardService = giftCardService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _addressModelFactory = addressModelFactory;
        _countryService = countryService;
        _storeMappingService = storeMappingService;
        _paymentService = paymentService;
        _webHelper = webHelper;
        _checkoutModelFactory = checkoutModelFactory;
        _permissionService = permissionService;
        _taxService = taxService;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _urlRecordService = urlRecordService;
        _productAttributeService = productAttributeService;
        _productAttributeParser = productAttributeParser;
        _shoppingCartSettings = shoppingCartSettings;
        _customerActivityService = customerActivityService;
        _nopUrlHelper = nopUrlHelper;
        _staticCacheManager = staticCacheManager;
    }

    #endregion Ctor

    #region Utilities

    private async Task<IList<AddressSelectListItem>> GetAvailableAddress(int customerId, int selectedAddressId, bool isNew)
    {
        var addresses = await (await _customerService.GetAddressesByCustomerIdAsync(customerId))
            .WhereAwait(async a => !a.CountryId.HasValue || await _countryService.GetCountryByAddressAsync(a) is Nop.Core.Domain.Directory.Country country &&
                (//published
                country.Published &&
                //allow billing
                country.AllowsBilling &&
                //enabled for the current store
                await _storeMappingService.AuthorizeAsync(country)))
            .ToListAsync();

        var items = new List<AddressSelectListItem>();
        foreach (var address in addresses)
        {
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings);

            var addressLine = "";
            addressLine += address.FirstName;
            addressLine += " " + address.LastName;
            if (addressModel.StreetAddressEnabled && !string.IsNullOrEmpty(addressModel.Address1))
            {
                addressLine += ", " + addressModel.Address1;
            }
            if (addressModel.CityEnabled && !string.IsNullOrEmpty(addressModel.City))
            {
                addressLine += ", " + addressModel.City;
            }
            if (addressModel.CountyEnabled && !string.IsNullOrEmpty(addressModel.County))
            {
                addressLine += ", " + addressModel.County;
            }
            if (addressModel.StateProvinceEnabled && !string.IsNullOrEmpty(addressModel.StateProvinceName))
            {
                addressLine += ", " + addressModel.StateProvinceName;
            }
            if (addressModel.ZipPostalCodeEnabled && !string.IsNullOrEmpty(addressModel.ZipPostalCode))
            {
                addressLine += " " + address.ZipPostalCode;
            }
            if (addressModel.CountryEnabled && !string.IsNullOrWhiteSpace(addressModel.CountryName))
            {
                addressLine += ", " + addressModel.CountryName;
            }
            items.Add(new AddressSelectListItem { Value = address.Id.ToString(), Text = addressLine, Selected = (address.Id == selectedAddressId) ? true : false, CountryId = address.CountryId.ToString() });
        }

        items.Add(new AddressSelectListItem { Value = "", Text = await _localizationService.GetResourceAsync("Checkout.NewAddress"), Selected = isNew ? true : false });
        return items;
    }

    private async Task RemoveNonValidDiscountAsync()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var discounts = await _customerService.ParseAppliedDiscountCouponCodesAsync(currentCustomer);
        var couponCodesToValidate = await _customerService.ParseAppliedDiscountCouponCodesAsync(currentCustomer);
        foreach (var aDiscount in discounts)
        {
            if ((await _discountService.GetAllDiscountsAsync(null, aDiscount, null))
                .FirstOrDefaultAwaitAsync(async d =>
                d.RequiresCouponCode && (await _discountService.ValidateDiscountAsync(d, currentCustomer, couponCodesToValidate)).IsValid) == null)
            {
                await _customerService.RemoveDiscountCouponCodeAsync(currentCustomer, aDiscount);
            }
        }
    }

    private async Task<IEnumerable<string>> ValidateAddress(AddressModel model, string customAttributes)
    {
        var validationResult = new AddressValidator(_localizationService, _stateProvinceService, _addressSettings, _customerSettings).Validate(model);
        List<string> list = new List<string>();
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                list.Add(error.ErrorMessage);
            }
        }

        if (customAttributes != null)
        {
            foreach (string attributeWarning in await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes))
            {
                list.Add(attributeWarning);
            }
        }
        return list;
    }

    private AddressModel ExtractAddressFromForm(IFormCollection form, string addresstype)
    {
        int? countryId = null;
        if (int.TryParse(form[addresstype + "CountryId"], out var result))
        {
            countryId = result;
        }
        int? stateProvinceId = null;
        if (int.TryParse(form[addresstype + "StateProvinceId"], out var result2))
        {
            stateProvinceId = result2;
        }

        AddressModel model = new AddressModel();
        model.FirstName = ((string)form[addresstype + "FirstName"]);
        model.LastName = ((string)form[addresstype + "LastName"]);
        model.Email = ((string)form[addresstype + "Email"]);
        model.Company = ((string)form[addresstype + "Company"]);
        model.CountryId = (countryId);
        model.StateProvinceId = (stateProvinceId);
        model.City = ((string)form[addresstype + "City"]);
        model.Address1 = ((string)form[addresstype + "Address1"]);
        model.Address2 = ((string)form[addresstype + "Address2"]);
        model.ZipPostalCode = ((string)form[addresstype + "ZipPostalCode"]);
        model.PhoneNumber = ((string)form[addresstype + "PhoneNumber"]);
        model.FaxNumber = ((string)form[addresstype + "FaxNumber"]);
        return model;
    }

    private async Task<Address> SaveOrUpdateAddress(AddressModel model, string customAttributes, string addressType)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentAddress = await _addressService.GetAddressByIdAsync(model.Id);

        if (currentAddress == null)
        {
            var currrentCustomerAddress = (await _customerService.GetAddressesByCustomerIdAsync(currentCustomer.Id)).ToList();
            var address = _addressService.FindAddress(currrentCustomerAddress, model.FirstName, model.LastName, model.PhoneNumber,
                                                   model.Email, model.FaxNumber, model.Company, model.Address1, model.Address2, model.City,
                                                   model.County, (model.StateProvinceId == 0 ? null : model.StateProvinceId), model.ZipPostalCode,
                                                   model.CountryId, customAttributes);
            if (address == null)
            {
                address = MappingExtensions.ToEntity(model, true);
                address.CustomAttributes = customAttributes;
                address.CreatedOnUtc = DateTime.UtcNow;
                address.CustomAttributes = customAttributes;
                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;

                await _addressService.InsertAddressAsync(address);

                await _customerService.InsertCustomerAddressAsync(currentCustomer, address);
            }

            if (addressType == "shipping")
                currentCustomer.ShippingAddressId = address.Id;
            else
                currentCustomer.BillingAddressId = address.Id;

            await _customerService.UpdateCustomerAsync(currentCustomer);

            return address;
        }
        currentAddress = model.ToEntity(currentAddress);
        currentAddress.CustomAttributes = customAttributes;
        await _addressService.UpdateAddressAsync(currentAddress);

        return currentAddress;
    }

    private async Task<string> ParseCustomAddressAttributes(IFormCollection form, string addressType)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = string.Empty;

        foreach (var attribute in await _addressAttributeService.GetAllAttributesAsync())
        {
            var controlId = string.Format("{0}_address_attribute_{1}", addressType, attribute.Id);
            var attributeValues = form[controlId];
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    if (!StringValues.IsNullOrEmpty(attributeValues) && int.TryParse(attributeValues, out var value) && value > 0)
                        attributesXml = AddAddressAttribute(attributesXml, attribute, value.ToString());
                    break;

                case AttributeControlType.Checkboxes:
                    foreach (var attributeValue in attributeValues.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(attributeValue, out value) && value > 0)
                            attributesXml = AddAddressAttribute(attributesXml, attribute, value.ToString());
                    }

                    break;

                case AttributeControlType.ReadonlyCheckboxes:
                    //load read-only (already server-side selected) values
                    var addressAttributeValues = await _addressAttributeService.GetAttributeValuesAsync(attribute.Id);
                    foreach (var addressAttributeValue in addressAttributeValues)
                    {
                        if (addressAttributeValue.IsPreSelected)
                            attributesXml = AddAddressAttribute(attributesXml, attribute, addressAttributeValue.Id.ToString());
                    }

                    break;

                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    if (!StringValues.IsNullOrEmpty(attributeValues))
                        attributesXml = AddAddressAttribute(attributesXml, attribute, attributeValues.ToString().Trim());
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

    private string AddAddressAttribute(string attributesXml, AddressAttribute attribute, string value)
    {
        var result = string.Empty;
        try
        {
            var xmlDoc = new XmlDocument();
            if (string.IsNullOrEmpty(attributesXml))
            {
                var element1 = xmlDoc.CreateElement("Attributes");
                xmlDoc.AppendChild(element1);
            }
            else
            {
                xmlDoc.LoadXml(attributesXml);
            }

            var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

            XmlElement attributeElement = null;
            //find existing
            var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/AddressAttribute");
            foreach (XmlNode node1 in nodeList1)
            {
                if (node1.Attributes?["ID"] == null)
                    continue;

                var str1 = node1.Attributes["ID"].InnerText.Trim();
                if (!int.TryParse(str1, out var id))
                    continue;

                if (id != attribute.Id)
                    continue;

                attributeElement = (XmlElement)node1;
                break;
            }

            //create new one if not found
            if (attributeElement == null)
            {
                attributeElement = xmlDoc.CreateElement("AddressAttribute");
                attributeElement.SetAttribute("ID", attribute.Id.ToString());
                rootElement.AppendChild(attributeElement);
            }

            var attributeValueElement = xmlDoc.CreateElement("AddressAttributeValue");
            attributeElement.AppendChild(attributeValueElement);

            var attributeValueValueElement = xmlDoc.CreateElement("Value");
            attributeValueValueElement.InnerText = value;
            attributeValueElement.AppendChild(attributeValueValueElement);

            result = xmlDoc.OuterXml;
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }

        return result;
    }

    private async Task SetPickupInStoreMethod(bool pickUpInStore, string pickUpPointId, string pickUpPointProvider)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        if (pickUpInStore)
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(currentCustomer);
            var selectedPickupPoint = (await _shippingService.GetPickupPointsAsync(cart, billingAddress, currentCustomer, pickUpPointProvider, currentStore.Id)).PickupPoints.ToList().FirstOrDefault(x => x.Id.Equals(pickUpPointId));
            if (selectedPickupPoint == null)
                throw new Exception("Pickup point is not allowed");

            var pickUpInStoreShippingOption = new ShippingOption
            {
                Name = string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), selectedPickupPoint.Name),
                Rate = selectedPickupPoint.PickupFee,
                Description = selectedPickupPoint.Description,
                ShippingRateComputationMethodSystemName = selectedPickupPoint.ProviderSystemName
            };
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, currentStore.Id);
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, selectedPickupPoint, currentStore.Id);
        }
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(currentCustomer);
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, (ShippingOption)null, currentStore.Id);
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, (PickupPoint)null, currentStore.Id);
            var shippingMethodModel = await _opcModelFactory.PrepareShippingMethodModelAsync(cart, shippingAddress);
            await SetDefaultShippingOptionsAsync(shippingMethodModel);
        }
    }

    private async Task<Address> SaveAddress(AddressModel model, string customAttributes)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currrentCustomerAddress = (await _customerService.GetAddressesByCustomerIdAsync(currentCustomer.Id)).ToList();
        var address = _addressService.FindAddress(currrentCustomerAddress, model.FirstName, model.LastName, model.PhoneNumber,
                                                    model.Email, model.FaxNumber, model.Company, model.Address1, model.Address2, model.City,
                                                    model.County, (model.StateProvinceId == 0 ? null : model.StateProvinceId), model.ZipPostalCode,
                                                    model.CountryId, customAttributes);
        if (address == null)
        {
            address = MappingExtensions.ToEntity(model, true);
            address.CustomAttributes = customAttributes;
            address.CreatedOnUtc = DateTime.UtcNow;
            address.CustomAttributes = customAttributes;
            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressService.InsertAddressAsync(address);

            await _customerService.InsertCustomerAddressAsync(currentCustomer, address);
        }

        return address;
    }

    private async Task SaveBillingAddress(AddressModel model, string customAttributes)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var savedBillingAddress = await SaveAddress(model, customAttributes);
        currentCustomer.BillingAddressId = savedBillingAddress.Id;
        await _customerService.UpdateCustomerAsync(currentCustomer);
    }

    private async Task SaveShippingAddress(AddressModel model, string customAttributes)
    {
        if (model != null)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var savedShippingAddress = await SaveAddress(model, customAttributes);
            currentCustomer.ShippingAddressId = savedShippingAddress.Id;
            await _customerService.UpdateCustomerAsync(currentCustomer);
        }
    }

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

    protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = string.Empty;
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, (await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
        foreach (var attribute in checkoutAttributes)
        {
            var controlId = $"checkout_attribute_{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;

                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }

                    break;

                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;

                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                    break;

                case AttributeControlType.Datepicker:
                    {
                        var date = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                        }
                        catch
                        {
                            // ignored
                        }

                        if (selectedDate.HasValue)
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                    }

                    break;

                case AttributeControlType.FileUpload:
                    {
                        Guid.TryParse(form[controlId], out var downloadGuid);
                        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                        if (download != null)
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                       attribute, download.DownloadGuid.ToString());
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        //validate conditional attributes (if specified)
        foreach (var attribute in checkoutAttributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributesXml);
            if (conditionMet.HasValue && !conditionMet.Value)
                attributesXml = _checkoutAttributeParser.RemoveAttribute(attributesXml, attribute.Id);
        }

        //save checkout attributes
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, (await _storeContext.GetCurrentStoreAsync()).Id);
    }

    protected virtual async Task<ShoppingCartModel.OrderReviewDataModel> PrepareOrderReviewDataModelAsync(IList<ShoppingCartItem> cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var model = new ShoppingCartModel.OrderReviewDataModel
        {
            Display = true
        };

        //billing info
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);
        if (billingAddress != null)
        {
            await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress,
                    address: billingAddress,
                    excludeProperties: false,
                    addressSettings: _addressSettings);
        }

        //shipping info
        if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            model.IsShippable = true;

            var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(customer,
                NopCustomerDefaults.SelectedPickupPointAttribute, store.Id);
            model.SelectedPickupInStore = _shippingSettings.AllowPickupInStore && pickupPoint != null;
            if (!model.SelectedPickupInStore)
            {
                if (await _customerService.GetCustomerShippingAddressAsync(customer) is Address address)
                {
                    await _addressModelFactory.PrepareAddressModelAsync(model.ShippingAddress,
                        address: address,
                        excludeProperties: false,
                        addressSettings: _addressSettings);
                }
            }
            else
            {
                var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);

                model.PickupAddress = new AddressModel
                {
                    Address1 = pickupPoint.Address,
                    City = pickupPoint.City,
                    County = pickupPoint.County,
                    CountryName = country?.Name ?? string.Empty,
                    StateProvinceName = state?.Name ?? string.Empty,
                    ZipPostalCode = pickupPoint.ZipPostalCode
                };
            }

            //selected shipping method
            var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);
            if (shippingOption != null)
                model.ShippingMethod = shippingOption.Name;
        }

        //payment info
        var selectedPaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
        var paymentMethod = await _paymentPluginManager
            .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName, customer, store.Id);
        model.PaymentMethod = paymentMethod != null
            ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id)
            : string.Empty;

        //custom values
        var processPaymentRequest = await _httpContextAccessor.HttpContext?.Session?.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
        if (processPaymentRequest != null)
            model.CustomValues = processPaymentRequest.CustomValues;

        return model;
    }

    protected virtual async Task<bool> IsMinimumOrderPlacementIntervalValidAsync(Customer customer)
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

    protected virtual async Task<IList<ShoppingCartModel.CheckoutAttributeModel>> PrepareCheckoutAttributeModelsAsync(
        IList<ShoppingCartItem> cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        var model = new List<ShoppingCartModel.CheckoutAttributeModel>();

        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var checkoutAttributes = await _checkoutAttributeService.
            GetAllAttributesAsync(_staticCacheManager, _storeMappingService, (await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
        foreach (var attribute in checkoutAttributes)
        {
            var attributeModel = new ShoppingCartModel.CheckoutAttributeModel
            {
                Id = attribute.Id,
                Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
                DefaultValue = await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue)
            };
            if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
            {
                attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            if (attribute.ShouldHaveValues)
            {
                //values
                var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    var attributeValueModel = new ShoppingCartModel.CheckoutAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                        ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                        IsPreSelected = attributeValue.IsPreSelected,
                    };
                    attributeModel.Values.Add(attributeValueModel);

                    //display price if allowed
                    if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
                    {
                        var (priceAdjustmentBase, _) = await _taxService.GetCheckoutAttributePriceAsync(attribute, attributeValue);
                        var priceAdjustment =
                            await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase,
                                await _workContext.GetWorkingCurrencyAsync());
                        if (priceAdjustmentBase > decimal.Zero)
                            attributeValueModel.PriceAdjustment =
                                "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment);
                        else if (priceAdjustmentBase < decimal.Zero)
                            attributeValueModel.PriceAdjustment =
                                "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment);
                    }
                }
            }

            //set already selected attributes
            var selectedCheckoutAttributes = await _genericAttributeService.GetAttributeAsync<string>(
                await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.Checkboxes:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                    {
                        if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
                        {
                            //clear default selection
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            //select new values
                            var selectedValues =
                                _checkoutAttributeParser.ParseAttributeValues(selectedCheckoutAttributes);
                            foreach (var attributeValue in await selectedValues.SelectMany(x => x.values).ToListAsync())
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                        item.IsPreSelected = true;
                        }
                    }

                    break;

                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //do nothing
                        //values are already pre-set
                    }

                    break;

                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
                        {
                            var enteredText =
                                _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                            if (enteredText.Any())
                                attributeModel.DefaultValue = enteredText[0];
                        }
                    }

                    break;

                case AttributeControlType.Datepicker:
                    {
                        //keep in mind my that the code below works only in the current culture
                        var selectedDateStr =
                            _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
                        if (selectedDateStr.Any())
                        {
                            if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                DateTimeStyles.None, out var selectedDate))
                            {
                                //successfully parsed
                                attributeModel.SelectedDay = selectedDate.Day;
                                attributeModel.SelectedMonth = selectedDate.Month;
                                attributeModel.SelectedYear = selectedDate.Year;
                            }
                        }
                    }

                    break;

                case AttributeControlType.FileUpload:
                    {
                        if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
                        {
                            var downloadGuidStr = _checkoutAttributeParser
                                .ParseValues(selectedCheckoutAttributes, attribute.Id).FirstOrDefault();
                            Guid.TryParse(downloadGuidStr, out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                                attributeModel.DefaultValue = download.DownloadGuid.ToString();
                        }
                    }

                    break;

                default:
                    break;
            }

            model.Add(attributeModel);
        }

        return model;
    }

    protected virtual async Task<CheckoutBillingAddressModel> PrepareCheckoutBillingAddressModelAsync(CheckoutBillingAddressModel model, string addressAttributes, Customer customer, IList<ShoppingCartItem> cart, Address address = null)
    {
        var newAddress = model.BillingNewAddress;

        var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
           selectedCountryId: address == null ? newAddress.CountryId : address.CountryId,
           overrideAttributesXml: addressAttributes);

        await _addressModelFactory.PrepareAddressModelAsync(model.BillingNewAddress,
               address: null,
               excludeProperties: false,
               addressSettings: _addressSettings,
               loadCountries: async () => await _countryService.GetAllCountriesForBillingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
               prePopulateWithCustomerFields: false,
               customer: customer,
               overrideAttributesXml: addressAttributes);

        billingAddressModel.BillingNewAddress = model.BillingNewAddress;

        return billingAddressModel;
    }

    protected virtual async Task<CheckoutShippingAddressModel> PrepareCheckoutShippingAddressModelAsync(CheckoutShippingAddressModel model, string addressAttributes, Customer customer, IList<ShoppingCartItem> cart, Address address = null)
    {
        var newAddress = model.ShippingNewAddress;
        var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                      selectedCountryId: address == null ? newAddress.CountryId : address.CountryId,
                      overrideAttributesXml: addressAttributes);

        await _addressModelFactory.PrepareAddressModelAsync(model.ShippingNewAddress,
               address: null,
               excludeProperties: false,
               addressSettings: _addressSettings,
               loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id),
               prePopulateWithCustomerFields: false,
               customer: customer,
               overrideAttributesXml: addressAttributes);

        shippingAddressModel.ShippingNewAddress = model.ShippingNewAddress;

        return shippingAddressModel;
    }

    protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
      ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
      DateTime? rentalEndDate, int quantity)
    {
        if (updatecartitem == null)
        {
            //add to the cart
            addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(await _workContext.GetCurrentCustomerAsync(),
                product, cartType, (await _storeContext.GetCurrentStoreAsync()).Id,
                attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity, true));
        }
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), updatecartitem.ShoppingCartType, (await _storeContext.GetCurrentStoreAsync()).Id);

            var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate);
            if (otherCartItemWithSameParameters != null &&
                otherCartItemWithSameParameters.Id == updatecartitem.Id)
            {
                //ensure it's some other shopping cart item
                otherCartItemWithSameParameters = null;
            }
            //update existing item
            addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                updatecartitem.Id, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
            if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
            {
                //delete the same shopping cart item (the other one)
                await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
            }
        }
    }

    protected virtual async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings, ShoppingCartType cartType,
        Product product)
    {
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart/wishlist
            //let's display warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    //if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                    //{
                    //    //redirect to the shopping cart page
                    //    return Json(new
                    //    {
                    //        redirect = Url.RouteUrl("ShoppingCart")
                    //    });
                    //}

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updateTopCartSectionHtml = string.Format(
                        await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
                        : string.Empty;

                    if (!_opcSettings.BypassShoppingCartPage)
                    {
                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                            Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml = updateTopCartSectionHtml,
                            updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
                            redirectbuynow = Url.RouteUrl("ShoppingCart")
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
                            Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml = updateTopCartSectionHtml,
                        updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml,
                        redirectbuynow = Url.RouteUrl("Opc")
                    });
                }
        }
    }

    #endregion Utilities

    #region Methods

    public virtual async Task<IActionResult> GetAddressById(int addressId, string addressType, bool isNew, bool isShipToSameAddress)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);
        if (address == null && !isNew)
            throw new ArgumentNullException(nameof(address));

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        if (addressType == "billing")
        {
            var billingAddressModel = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, _opcSettings.DefaultBillingAddressCountryId);
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                address: isNew ? null : address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesForBillingAsync((await _workContext.GetWorkingLanguageAsync()).Id));

            billingAddressModel.BillingNewAddress = addressModel;

            if (_opcSettings.PreselectPreviousBillingAddress && (await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0)) != null)
            {
                billingAddressModel.CustomProperties.Add("CustomerBillingAddress", customer.BillingAddressId?.ToString());
            }

            if (isNew)
            {
                var isSessionExits = _httpContextAccessor.HttpContext.Session.TryGetValue(OpcDefaults.BillingAddressSessionKey, out AddressModel billingAddress);

                if (isSessionExits)
                {
                    billingAddressModel.BillingNewAddress = billingAddress;
                }
                else
                {
                    await _httpContextAccessor.HttpContext.Session.SetAsync(OpcDefaults.BillingAddressSessionKey, billingAddressModel.BillingNewAddress);
                }

                billingAddressModel.NewAddressPreselected = true;
            }

            billingAddressModel.ShipToSameAddress = isShipToSameAddress;
            if (billingAddressModel.ShipToSameAddress)
            {
                customer.ShippingAddressId = customer.BillingAddressId;
                await _customerService.UpdateCustomerAsync(customer);
            }
            return Json(new { error = false, html = await RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel) });
        }

        var shippingAddressModel = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart, _opcSettings.DefaultShippingAddressCountryId);
        var newAddressModel = new AddressModel();
        await _addressModelFactory.PrepareAddressModelAsync(newAddressModel,
               address: isNew ? null : address,
               excludeProperties: false,
               addressSettings: _addressSettings,
               loadCountries: async () => await _countryService.GetAllCountriesForShippingAsync((await _workContext.GetWorkingLanguageAsync()).Id));

        shippingAddressModel.ShippingNewAddress = newAddressModel;

        if (_opcSettings.PreselectPreviousShippingAddress && (await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0)) != null)
        {
            shippingAddressModel.CustomProperties.Add("CustomerShippingAddress", customer.ShippingAddressId?.ToString());
        }

        if (isNew)
        {
            var isSessionExists = _httpContextAccessor.HttpContext.Session.TryGetValue(OpcDefaults.ShippingAddressSessionKey, out AddressModel shippingAddress);

            if (isSessionExists)
            {
                shippingAddressModel.ShippingNewAddress = shippingAddress;
            }
            else
            {
                await _httpContextAccessor.HttpContext.Session.SetAsync(OpcDefaults.ShippingAddressSessionKey, shippingAddressModel.ShippingNewAddress);
            }

            shippingAddressModel.NewAddressPreselected = true;
        }

        return Json(new { error = false, html = await RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddressModel) });
    }

    [HttpPost]
    public virtual async Task<IActionResult> SetBillingAddress(int billingAddressId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (billingAddressId > 0)
        {
            var billingAddress = await _addressService.GetAddressByIdAsync(billingAddressId);
            customer.BillingAddressId = billingAddress.Id;
            await _customerService.UpdateCustomerAsync(customer);
        }
        else
        {
            customer.BillingAddressId = null;
            await _customerService.UpdateCustomerAsync(customer);
        }
        return Json(null);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SetShippingAddress(int addressId)
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (addressId > 0)
            {
                var address = await _addressService.GetAddressByIdAsync(addressId);
                if (address == null)
                    throw new Exception("Address can't be loaded");
                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);
            }
            else
            {
                customer.ShippingAddressId = null;
                await _customerService.UpdateCustomerAsync(customer);
            }

            return Json(new { error = false });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = true, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    public JsonResult UpdateBillingAddress(CheckoutBillingAddressModel model, IFormCollection form)
    {
        lock (_addressLocker)
        {
            var customer = _workContext.GetCurrentCustomerAsync().Result;
            var store = _storeContext.GetCurrentStoreAsync().Result;
            var cart = _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id).Result;

            int.TryParse(form["billing_address_id"], out int billingAddressId);
            if (billingAddressId < 1)
            {
                var newAddressCustomAttributes = ParseCustomAddressAttributes(form, "BillingNewAddress").Result;
                var newAddressCustomAttributeWarning = _addressAttributeParser.GetAttributeWarningsAsync(newAddressCustomAttributes).Result;

                foreach (var error in newAddressCustomAttributeWarning)
                {
                    ModelState.AddModelError("", error);
                }

                if (!ModelState.IsValid)
                {
                    var billingAddressModel = PrepareCheckoutBillingAddressModelAsync(model, newAddressCustomAttributes, customer, cart).Result;
                    billingAddressModel.NewAddressPreselected = true;
                    HttpContext.Session.SetAsync(OpcDefaults.BillingAddressSessionKey, billingAddressModel.BillingNewAddress);

                    return Json(new { error = true, html = RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel).Result, message = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var updatedAddress = SaveOrUpdateAddress(model.BillingNewAddress, newAddressCustomAttributes, "billing").Result;
                HttpContext.Session.Remove(OpcDefaults.BillingAddressSessionKey);

                var savedBillingAddressModel = PrepareCheckoutBillingAddressModelAsync(model, newAddressCustomAttributes, customer, cart).Result;

                if (_opcSettings.PreselectPreviousBillingAddress && (_addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0).Result) != null)
                {
                    savedBillingAddressModel.CustomProperties.Add("CustomerBillingAddress", customer.BillingAddressId?.ToString());
                }

                if (form["ShipToSameAddress"].FirstOrDefault() == "true")
                {
                    customer.ShippingAddressId = customer.BillingAddressId;
                    _customerService.UpdateCustomerAsync(customer).Wait();
                }

                return Json(new { error = false, html = RenderPartialViewToStringAsync("OpcBillingAddress", savedBillingAddressModel).Result, addressId = updatedAddress.Id, selectlist = GetAvailableAddress(customer.Id, updatedAddress.Id, false).Result });
            }

            var address = _customerService.GetCustomerAddressAsync(customer.Id, billingAddressId).Result;
            if (address == null)
                throw new Exception("Address can't be loaded");

            //custom address attributes
            var customAttributes = ParseCustomAddressAttributes(form, "BillingNewAddress").Result;
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarningsAsync(customAttributes).Result;

            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (!ModelState.IsValid)
            {
                var billingAddressModel = PrepareCheckoutBillingAddressModelAsync(model, customAttributes, customer, cart, address).Result;
                if (_opcSettings.PreselectPreviousBillingAddress && (_addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0).Result) != null)
                {
                    billingAddressModel.CustomProperties.Add("CustomerBillingAddress", customer.BillingAddressId?.ToString());
                }

                return Json(new { error = true, html = RenderPartialViewToStringAsync("OpcBillingAddress", billingAddressModel).Result, message = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            address = model.BillingNewAddress.ToEntity(address);
            address.CustomAttributes = customAttributes;
            _addressService.UpdateAddressAsync(address).Wait();

            customer.BillingAddressId = address.Id;

            _customerService.UpdateCustomerAsync(customer).Wait();

            var billingAddresssModel = PrepareCheckoutBillingAddressModelAsync(model, customAttributes, customer, cart, address).Result;
            billingAddresssModel.BillingNewAddress = model.BillingNewAddress;

            if (_opcSettings.PreselectPreviousBillingAddress && (_addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0).Result) != null)
            {
                billingAddresssModel.CustomProperties.Add("CustomerBillingAddress", customer.BillingAddressId?.ToString());
            }

            if (form["ShipToSameAddress"].FirstOrDefault() == "true")
            {
                customer.ShippingAddressId = customer.BillingAddressId;
                _customerService.UpdateCustomerAsync(customer).Wait();
            }

            return Json(new { error = false, html = RenderPartialViewToStringAsync("OpcBillingAddress", billingAddresssModel).Result, selectlist = GetAvailableAddress(customer.Id, billingAddressId, false).Result });
        }
    }

    [HttpPost]
    public JsonResult UpdateShippingAddress(CheckoutShippingAddressModel model, IFormCollection form)
    {
        lock (_addressLocker)
        {
            var customer = _workContext.GetCurrentCustomerAsync().Result;
            var store = _storeContext.GetCurrentStoreAsync().Result;
            var cart = _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id).Result;

            int.TryParse(form["shipping_address_id"], out int shippingAddressId);

            if (shippingAddressId < 1)
            {
                var newAddressCustomAttributes = ParseCustomAddressAttributes(form, "ShippingNewAddress").Result;
                var newAddressCustomAttributeWarning = _addressAttributeParser.GetAttributeWarningsAsync(newAddressCustomAttributes).Result;

                foreach (var error in newAddressCustomAttributeWarning)
                {
                    ModelState.AddModelError("", error);
                }
                if (!ModelState.IsValid)
                {
                    var shippingAddressModel = PrepareCheckoutShippingAddressModelAsync(model, newAddressCustomAttributes, customer, cart).Result;

                    shippingAddressModel.NewAddressPreselected = true;

                    HttpContext.Session.SetAsync(OpcDefaults.ShippingAddressSessionKey, model.ShippingNewAddress).GetAwaiter().GetResult();
                    return Json(new { error = true, html = RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddressModel).Result, message = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var updatedAddress = SaveOrUpdateAddress(model.ShippingNewAddress, newAddressCustomAttributes, "shipping").Result;

                var savedShippingAddressModel = PrepareCheckoutShippingAddressModelAsync(model, newAddressCustomAttributes, customer, cart).Result;
                HttpContext.Session.Remove(OpcDefaults.ShippingAddressSessionKey);

                if (_opcSettings.PreselectPreviousShippingAddress && (_addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0).Result) != null)
                {
                    savedShippingAddressModel.CustomProperties.Add("CustomerShippingAddress", customer.ShippingAddressId?.ToString());
                }

                return Json(new { error = false, html = RenderPartialViewToStringAsync("OpcShippingAddress", savedShippingAddressModel).Result, addressId = updatedAddress.Id, selectlist = GetAvailableAddress(customer.Id, updatedAddress.Id, false).Result });
            }

            var address = _customerService.GetCustomerAddressAsync(customer.Id, shippingAddressId).Result;
            if (address == null)
                throw new Exception("Address can't be loaded");

            //custom address attributes
            var customAttributes = ParseCustomAddressAttributes(form, "ShippingNewAddress").Result;
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarningsAsync(customAttributes).Result;

            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }
            if (!ModelState.IsValid)
            {
                var shippingAddressModel = PrepareCheckoutShippingAddressModelAsync(model, customAttributes, customer, cart, address).Result;

                if (_opcSettings.PreselectPreviousShippingAddress && (_addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0).Result) != null)
                {
                    shippingAddressModel.CustomProperties.Add("CustomerShippingAddress", customer.ShippingAddressId?.ToString());
                }

                return Json(new { error = true, html = RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddressModel).Result, message = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            address = model.ShippingNewAddress.ToEntity(address);
            address.CustomAttributes = customAttributes;

            _addressService.UpdateAddressAsync(address).Wait();

            customer.ShippingAddressId = address.Id;
            _customerService.UpdateCustomerAsync(customer).Wait();

            var shippingAddresssModel = PrepareCheckoutShippingAddressModelAsync(model, customAttributes, customer, cart, address).Result;

            if (_opcSettings.PreselectPreviousShippingAddress && (_addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0).Result) != null)
            {
                shippingAddresssModel.CustomProperties.Add("CustomerShippingAddress", customer.ShippingAddressId?.ToString());
            }

            return Json(new { error = false, html = RenderPartialViewToStringAsync("OpcShippingAddress", shippingAddresssModel).Result, selectlist = GetAvailableAddress(customer.Id, shippingAddressId, false).Result });
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> LoadOrderReview()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
        var orderReview = await PrepareOrderReviewDataModelAsync(cart);
        var html = await RenderPartialViewToStringAsync("_OrderReviewData", orderReview);
        return Json(new
        {
            update_section = new
            {
                name = "order-review",
                html = html
            }
        });
    }

    [HttpGet]
    public virtual async Task<IActionResult> LoadShippingMethod()
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(currentCustomer);
            var isShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);

            if (!isShippingRequired)
                return Json(new
                {
                    update_section = new
                    {
                        name = "shipping-method",
                        html = await _localizationService.GetResourceAsync("Checkout.ShippingMethodNotRequired"),
                        shippingrequired = false
                    }
                });

            var shippingMethodModel = await _opcModelFactory.PrepareShippingMethodModelAsync(cart, shippingAddress);
            var html = await RenderPartialViewToStringAsync("OpcShippingMethods", shippingMethodModel);
            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne && shippingMethodModel.ShippingMethods.Count == 1)
            {
                await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingMethodModel.ShippingMethods.First().ShippingOption, currentStore.Id);
                html = string.Empty;
            }
            return Json(new
            {
                update_section = new
                {
                    name = "shipping-method",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpGet]
    public virtual async Task<IActionResult> LoadShippingMethodByAddress(int addressId)
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var shippingAddress = (await _customerService.GetAddressesByCustomerIdAsync(currentCustomer.Id)).Where(a => a.Id == addressId).FirstOrDefault();
            if (shippingAddress == null)
                throw new Exception("No shipping address found");
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            var isShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);

            if (!isShippingRequired)
                return Json(new
                {
                    update_section = new
                    {
                        name = "shipping-method",
                        html = await _localizationService.GetResourceAsync("Checkout.ShippingMethodNotRequired"),
                        shippingrequired = false
                    }
                });
            var shippingMethodModel = await _opcModelFactory.PrepareShippingMethodModelAsync(cart, shippingAddress);
            var html = await RenderPartialViewToStringAsync("OpcShippingMethods", shippingMethodModel);

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne && shippingMethodModel.ShippingMethods.Count == 1)
            {
                await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingMethodModel.ShippingMethods.First().ShippingOption, currentStore.Id);
                html = string.Empty;
            }
            return Json(new
            {
                update_section = new
                {
                    name = "shipping-method",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    public virtual async Task<IActionResult> UpdateShippingMethod(UpdateShippingMethodModel model)
    {
        try
        {
            if (model.ShippingMethodSystemName != null)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(currentCustomer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, currentStore.Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(currentCustomer),
                        currentCustomer, model.ShippingMethodSystemName, currentStore.Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(model.ShippingMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(model.SelectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, currentStore.Id);
            }
            return Json(new { error = 0 });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> SetPickUpInStore(SetPickupInStoreModel model)
    {
        if (_shippingSettings.AllowPickupInStore)
        {
            var pickupPoint = model.PickUpPoint.ToString().Split(new[] { "___" }, StringSplitOptions.None);
            var pickupPointValue = pickupPoint[0];
            var pickupPointProviderName = pickupPoint[1];
            await SetPickupInStoreMethod(model.PickUpInStore, pickupPointValue, pickupPointProviderName);
        }
        return Json(null);
    }

    [HttpGet]
    public virtual async Task<IActionResult> LoadPaymentMethods(int countryId)
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            var paymentMethodModel = await _opcModelFactory.PreparePaymentMethodsModelAsync(cart, countryId);
            var isPaymentWorkFlowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, null);
            var selectedPaymentSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);
            return Json(new
            {
                update_section = new
                {
                    name = "payment-method",
                    html = await RenderPartialViewToStringAsync("OpcPaymentMethods", paymentMethodModel),
                    paymentMethodSystemName = selectedPaymentSystemName,
                    ispaymentworkflowrequired = isPaymentWorkFlowRequired
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> UpdatePayment(UpdatePaymentMethodModel model)
    {
        try
        {
            if (model.PaymentMethodSystemName != null)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPaymentMethodAttribute, model.PaymentMethodSystemName, (await _storeContext.GetCurrentStoreAsync()).Id);
            }
            return Json(new
            {
                paymentMethodSystemName = model.PaymentMethodSystemName
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    public virtual async Task<IActionResult> LoadPaymentInfo(string paymentMethodSystemName)
    {
        try
        {
            var paymentMethodInst = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                throw new Exception("Selected payment method can't be parsed");

            var html = string.Empty;

            if (paymentMethodInst.SkipPaymentInfo ||
                (paymentMethodInst.PaymentMethodType == PaymentMethodType.Redirection
                && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                return Json(new
                {
                    update_section = new
                    {
                        name = "payment-info",
                        html = html
                    }
                });
            }

            var paymentInfoModel = await _opcModelFactory.PreparePaymentInfoModelAsync(paymentMethodInst);
            return Json(new
            {
                update_section = new
                {
                    name = "payment-info",
                    html = await RenderPartialViewToStringAsync("OpcPaymentInfo", paymentInfoModel)
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> UseRewardPoints(UseRewardPointsModel model)
    {
        if (_rewardPointsSettings.Enabled)
        {
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute, model.UseRewardPoints, (await _storeContext.GetCurrentStoreAsync()).Id);
        }
        return Json(null);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> ApplyGiftCard(GiftCardModel giftCardModel)
    {
        try
        {
            //trim
            var giftcardcouponcode = giftCardModel.GiftCardCouponCode;
            if (giftcardcouponcode != null)
                giftcardcouponcode = giftcardcouponcode.Trim();

            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new ShoppingCartModel();
            if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            {
                if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftcardcouponcode)).FirstOrDefault();
                    var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                    if (isGiftCardValid)
                    {
                        await _customerService.ApplyGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), giftcardcouponcode);
                        model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                        model.GiftCardBox.IsApplied = true;
                    }
                    else
                    {
                        model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.GiftCardBox.IsApplied = false;
                    }
                }
                else
                {
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.GiftCardBox.IsApplied = false;
            }

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            var html = await RenderPartialViewToStringAsync("_GiftCardBox", model.GiftCardBox);
            return Json(new
            {
                update_section = new
                {
                    name = "gift-card",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> RemoveGiftCardCode(GiftCardModel giftCardModel)
    {
        try
        {
            var model = new ShoppingCartModel();

            //get gift card identifier
            var giftCardId = 0;
            giftCardId = giftCardModel.GiftCardId;
            var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
            if (gc != null)
                await _customerService.RemoveGiftCardCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), gc.GiftCardCouponCode);

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            var html = await RenderPartialViewToStringAsync("_GiftCardBox", model.GiftCardBox);
            return Json(new
            {
                update_section = new
                {
                    name = "gift-card",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> ApplyDiscountCoupon(DiscountCouponCodeModel couponCodeModel)
    {
        try
        {
            //trim
            var discountcouponcode = couponCodeModel.DiscountCouponCode;
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var model = new ShoppingCartModel();
            if (!string.IsNullOrWhiteSpace(discountcouponcode))
            {
                //we find even hidden records here. this way we can display a user-friendly message if it's expired
                var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                    .Where(d => d.RequiresCouponCode)
                    .ToList();
                if (discounts.Any())
                {
                    var userErrors = new List<string>();
                    var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                    {
                        var validationResult = await _discountService.ValidateDiscountAsync(discount, await _workContext.GetCurrentCustomerAsync(), new[] { discountcouponcode });
                        userErrors.AddRange(validationResult.Errors);

                        return validationResult.IsValid;
                    });

                    if (anyValidDiscount)
                    {
                        //valid
                        await _customerService.ApplyDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discountcouponcode);
                        model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                        model.DiscountBox.IsApplied = true;
                    }
                    else
                    {
                        if (userErrors.Any())
                            //some user errors
                            model.DiscountBox.Messages = userErrors;
                        else
                            //general error text
                            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                    }
                }
                else
                    //discount cannot be found
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
            }
            else
                //empty coupon code
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            var html = await RenderPartialViewToStringAsync("_DiscountBox", model.DiscountBox);
            return Json(new
            {
                update_section = new
                {
                    name = "discount-box",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> RemoveDiscountCoupon(DiscountCouponCodeModel discountModel)
    {
        try
        {
            var model = new ShoppingCartModel();

            //get discount identifier
            var discountId = 0;
            discountId = discountModel.DiscountId;

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount != null)
                await _customerService.RemoveDiscountCouponCodeAsync(await _workContext.GetCurrentCustomerAsync(), discount.CouponCode);

            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
            var html = await RenderPartialViewToStringAsync("_DiscountBox", model.DiscountBox);
            return Json(new
            {
                update_section = new
                {
                    name = "discount-box",
                    html = html
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    public virtual async Task<IActionResult> GetOrderTotals(bool isEditable = false)
    {
        var html = await RenderViewComponentToStringAsync(typeof(OpcOrderTotalsViewComponent), isEditable);

        return Json(new
        {
            update_section = new
            {
                name = "order-total-custom",
                html = html
            }
        });
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetShoppingCartItems()
    {
        var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, shoppingCartItems, _opcSettings.IsShoppingCartEditable);

        var html = await RenderPartialViewToStringAsync("ShoppingCartItems", model);
        return Json(new
        {
            update_section = new
            {
                name = "shopping-cart-items",
                html = html
            }
        });
    }

    public virtual async Task<IActionResult> GetFlyOutCart()
    {
        return await Task.FromResult(ViewComponent("FlyoutShoppingCart"));
    }

    public virtual async Task<IActionResult> UpdateShoppingCartItem(ShoppingCartItemModel shoppingCartItemModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        IList<string> warnings = new List<string>();
        if (shoppingCartItemModel.CartItemQuantity == 0)
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Opc.ShoppingCart.Item.Error"));
            return Json(new
            {
                warnings = warnings
            });
        }
        var shoppingCartItem = (await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id))
                                    .ToList().FirstOrDefault(x => x.Id == shoppingCartItemModel.CartItemId);
        if (shoppingCartItem != null)
        {
            warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(currentCustomer, shoppingCartItem.Id, shoppingCartItem.AttributesXml, shoppingCartItem.CustomerEnteredPrice,
                                                                                shoppingCartItem.RentalStartDateUtc, shoppingCartItem.RentalEndDateUtc, shoppingCartItemModel.CartItemQuantity, false);
        }

        var currentCartCount = (await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id)).Sum(item => item.Quantity);

        return Json(new
        {
            warnings = warnings,
            count = currentCartCount
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> DeleteShoppingCartItem(ShoppingCartItemModel shoppingCartItemModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
        var cartItemDeleteRequest = cart.FirstOrDefault(x => x.Id == shoppingCartItemModel.CartItemId);
        if (cartItemDeleteRequest != null)
            await _shoppingCartService.DeleteShoppingCartItemAsync(cartItemDeleteRequest, false, true);

        var cartItemCount = (await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id)).Sum(item => item.Quantity);

        if (cartItemCount == 0)
            await _genericAttributeService.SaveAttributeAsync<string>(currentCustomer, NopCustomerDefaults.CheckoutAttributes, null, currentStore.Id);

        return Json(new
        {
            CartCount = cartItemCount
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> CheckoutAttributeChange(IFormCollection form, bool isEditable)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        //save selected attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);
        var attributeXml = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
            NopCustomerDefaults.CheckoutAttributes, (await _storeContext.GetCurrentStoreAsync()).Id);

        //conditions
        var enabledAttributeIds = new List<int>();
        var disabledAttributeIds = new List<int>();
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var attributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, (await _storeContext.GetCurrentStoreAsync()).Id, excludeShippableAttributes);
        foreach (var attribute in attributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributeXml);
            if (conditionMet.HasValue)
            {
                if (conditionMet.Value)
                    enabledAttributeIds.Add(attribute.Id);
                else
                    disabledAttributeIds.Add(attribute.Id);
            }
        }

        //update blocks
        var ordetotalssectionhtml = await RenderViewComponentToStringAsync(typeof(OpcOrderTotalsViewComponent), new { _opcSettings.IsShoppingCartEditable });
        var selectedcheckoutattributesssectionhtml = await RenderViewComponentToStringAsync(typeof(SelectedCheckoutAttributesViewComponent));

        return Json(new
        {
            ordetotalssectionhtml,
            selectedcheckoutattributesssectionhtml,
            enabledattributeids = enabledAttributeIds.ToArray(),
            disabledattributeids = disabledAttributeIds.ToArray()
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> GetEstimateShipping(int? countryId, int? stateProvinceId, string zipPostalCode, string city)
    {
        var model = new EstimateShippingModel();
        model.CountryId = countryId;
        model.StateProvinceId = stateProvinceId;
        model.ZipPostalCode = zipPostalCode;
        model.City = city;
        var errors = new StringBuilder();

        if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(zipPostalCode))
            errors.Append(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

        if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(city))
        {
            if (errors.Length > 0)
                errors.Append("<br>");
            errors.Append(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));
        }

        if (countryId == null || countryId == 0)
        {
            if (errors.Length > 0)
                errors.Append("<br>");
            errors.Append(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));
        }

        if (errors.Length > 0)
        {
            return Content(errors.ToString());
        }

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var result = await _opcModelFactory.PrepareEstimateShippingResultModelAsync(cart, model, true);
        return PartialView("_EstimateShippingResult", result);
    }

    [HttpGet]
    public virtual async Task<IActionResult> LoadCheckoutAttributes()
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);
            if (!cart.Any())
                throw new Exception("Your cart is empty");

            var shoppingCartModel = new ShoppingCartModel();
            shoppingCartModel.CheckoutAttributes = await PrepareCheckoutAttributeModelsAsync(cart);
            var html = await RenderPartialViewToStringAsync("_CustomCheckoutAttributes", shoppingCartModel);
            var selectedCheckoutAttribute = await RenderViewComponentToStringAsync(typeof(SelectedCheckoutAttributesViewComponent));
            var count = shoppingCartModel.CheckoutAttributes.Count;

            return Json(new
            {
                update_section = new
                {
                    name = "checkout-attribute",
                    html = html,
                    selectedcheckoutattributename = "selected-checkout-attribute",
                    selectedcheckoutattributehtml = selectedCheckoutAttribute,
                    attributeCount = count
                }
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, message = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    public virtual async Task<IActionResult> Checkout()
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return RedirectToRoute("Homepage");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        if (!cart.Any())
            return RedirectToRoute("Homepage");

        if (!_opcSettings.EnableOnePageCheckout)
            return RedirectToRoute("Checkout");

        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
        var downloadableProductsRequireRegistration =
            _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

        if (await _customerService.IsGuestAsync(customer) && (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
            return Challenge();
        else if (await _customerService.IsGuestAsync(customer))
        {
            if (!(HttpContext.Session.GetString("CheckoutRedirect")?.Equals("true") ?? false))
            {
                HttpContext.Session.SetString("CheckoutRedirect", "true");
                return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("Opc") });
            }
        }
        HttpContext.Session.Remove("CheckoutRedirect");
        await _customerService.ResetCheckoutDataAsync(customer, (await _storeContext.GetCurrentStoreAsync()).Id, false, false, true, true, true);
        await RemoveNonValidDiscountAsync();
        var model = await _opcModelFactory.PrepareOpcModelAsync(cart);

        return View(model);
    }

    public virtual async Task<IActionResult> ConfirmOrder(IFormCollection form)
    {
        try
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, currentStore.Id);



            int.TryParse(form["billing_address_id"], out int billingAddressId);
            int.TryParse(form["shipping_address_id"], out int shippingAddressId);

            bool.TryParse(form["DisableBillingAddressCheckoutStep"], out bool disableBillingAddressCheckoutStep);

            AddressModel billingAddressModel = new AddressModel();
            AddressModel shippingAddressModel = new AddressModel();

            List<string> validationErrors = new List<string>();

            //Check minimum order place interval
            if (!await IsMinimumOrderPlacementIntervalValidAsync(currentCustomer))
            {
                validationErrors.Add(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));
            }

            if (billingAddressId > 0)
            {
                if (!disableBillingAddressCheckoutStep)
                {
                    var address = (await _customerService.GetAddressesByCustomerIdAsync(currentCustomer.Id)).FirstOrDefault(a => a.Id == billingAddressId);
                    if (address == null)
                        throw new Exception("Address can't be loaded");

                    billingAddressModel = ExtractAddressFromForm(form, "BillingNewAddress.");
                    var customAttributesOfBillingAddress = await ParseCustomAddressAttributes(form, "BillingNewAddress");
                    validationErrors.AddRange(await ValidateAddress(billingAddressModel, customAttributesOfBillingAddress));

                    if (validationErrors.Count() > 0)
                        return Json(new
                        {
                            error = 1,
                            Errors = validationErrors
                        });

                    await SaveBillingAddress(billingAddressModel, customAttributesOfBillingAddress);
                }
            }
            else
            {
                validationErrors.Add(await _localizationService.GetResourceAsync("NopStation.Opc.Billing.NotSet"));
                return Json(new
                {
                    error = 1,
                    Errors = validationErrors
                });
            }

            var pickupinstore = form["PickupInStore"];
            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart) && !(form["PickupInStore"].FirstOrDefault() == "true" || form["PickupInStore"].FirstOrDefault() == "True"))
            {
                if (form["ShipToSameAddress"].FirstOrDefault() == "true")
                {
                    currentCustomer.ShippingAddressId = currentCustomer.BillingAddressId;
                    await _customerService.UpdateCustomerAsync(currentCustomer);
                }
                else
                {
                    if (shippingAddressId > 0)
                    {
                        var address = (await _customerService.GetAddressesByCustomerIdAsync(currentCustomer.Id)).FirstOrDefault(a => a.Id == shippingAddressId);
                        if (address == null)
                            throw new Exception("Address can't be loaded");

                        shippingAddressModel = ExtractAddressFromForm(form, "ShippingNewAddress.");
                        var customAttributesOfShippingAddress = await ParseCustomAddressAttributes(form, "ShippingNewAddress");
                        validationErrors.AddRange(await ValidateAddress(shippingAddressModel, customAttributesOfShippingAddress));
                        if (validationErrors.Count > 0)
                        {
                            return Json(new
                            {
                                error = 1,
                                Errors = validationErrors
                            });
                        }
                        await SaveShippingAddress(shippingAddressModel, customAttributesOfShippingAddress);
                    }
                    else
                    {
                        validationErrors.Add(await _localizationService.GetResourceAsync("NopStation.Opc.Shipping.NotSet"));
                        return Json(new
                        {
                            error = 1,
                            Errors = validationErrors
                        });
                    }
                }
            }

            var selectedPaymentMethod = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);
            ProcessPaymentRequest processPaymentRequest = new ProcessPaymentRequest();

            var loadedPaymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(selectedPaymentMethod, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);

            if (loadedPaymentMethod != null)
            {
                processPaymentRequest = await loadedPaymentMethod.GetPaymentInfoAsync(form);
                validationErrors.AddRange(await loadedPaymentMethod.ValidatePaymentFormAsync(form));
            }
            else if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, null))
            {
                validationErrors.Add("Payment method not available");
            }

            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart) && (await _genericAttributeService.GetAttributeAsync<ShippingOption>(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, currentStore.Id) == null))
            {
                validationErrors.Add("Shipping method not available");
            }

            //CheckoutAttribute
            await ParseAndSaveCheckoutAttributesAsync(cart, form);
            string checkoutAttribute = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, NopCustomerDefaults.CheckoutAttributes, currentStore.Id);
            IList<string> shoppingCartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttribute, true);

            validationErrors.AddRange(shoppingCartWarnings);

            if (validationErrors.Count > 0)
            {
                return Json(new
                {
                    error = 1,
                    Errors = validationErrors
                });
            }

            //place order
            await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
            processPaymentRequest.StoreId = currentStore.Id;
            processPaymentRequest.CustomerId = currentCustomer.Id;
            processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, currentStore.Id);

            var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);

            if (placeOrderResult.Success)
            {
                var redirectUrl = Url.RouteUrl("CheckoutCompleted", new
                {
                    orderId = placeOrderResult.PlacedOrder.Id
                });

                //payment method
                if (loadedPaymentMethod != null)
                {
                    if (loadedPaymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                    {
                        PostProcessPaymentRequest postProcessPayment = new PostProcessPaymentRequest();
                        postProcessPayment.Order = placeOrderResult.PlacedOrder;
                        await _paymentService.PostProcessPaymentAsync(postProcessPayment);
                    }
                    else
                    {
                        redirectUrl = Url.Action("PostProcessPaymentAfterRedirect", "Opc", new
                        {
                            orderId = placeOrderResult.PlacedOrder.Id
                        });
                    }
                }

                return Json(new
                {
                    success = 1,
                    RedirectUrl = redirectUrl
                });
            }

            return Json(new
            {
                error = 1,
                Errors = placeOrderResult.Errors
            });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return Json(new { error = 1, Exc = await _localizationService.GetResourceAsync("NopStation.Opc.Common.Error") });
        }
    }

    public virtual async Task<IActionResult> PostProcessPaymentAfterRedirect(int orderId = 0)
    {
        try
        {
            if (orderId == 0)
                return RedirectToRoute("HomePage");

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
                return RedirectToRoute("HomePage");

            var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
            if (paymentMethod == null)
                return RedirectToRoute("HomePage");
            if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                return RedirectToRoute("HomePage");

            //ensure that order has been just placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                return RedirectToRoute("HomePage");

            //Redirection will not work on one page checkout page because it's AJAX request.
            //That's why we process it here
            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };

            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content(await _localizationService.GetResourceAsync("Checkout.RedirectMessage"));
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return RedirectToRoute("HomePage");
        }
    }
    #region Buy Now

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> AddProductToCartCheckout_Catalog(int productId, int shoppingCartTypeId,
        int quantity, bool forceredirection = false)
    {
        var cartType = (ShoppingCartType)shoppingCartTypeId;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            //no product found
            return Json(new
            {
                success = false,
                message = "No product found with the specified ID"
            });

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        //products with "minimum order quantity" more than a specified qty
        if (product.OrderMinimumQuantity > quantity)
        {
            //we cannot add to the cart such products from category pages
            //it can confuse customers. That's why we redirect customers to the product details page
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        if (product.CustomerEntersPrice)
        {
            //cannot be added to the cart (requires a customer to enter price)
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        if (product.IsRental)
        {
            //rental products require start/end dates to be entered
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        if (allowedQuantities.Length > 0)
        {
            //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
        var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
        {
            //product has some attributes. let a customer see them
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        //creating XML for "read-only checkboxes" attributes
        var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
        {
            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
            foreach (var selectedAttributeId in attributeValues
                .Where(v => v.IsPreSelected)
                .Select(v => v.Id)
                .ToList())
            {
                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                    attribute, selectedAttributeId.ToString());
            }

            return attributesXml;
        });

        //get standard warnings without attribute validations
        //first, try to find existing shopping cart item
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), cartType, (await _storeContext.GetCurrentStoreAsync()).Id);
        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
        //if we already have the same product in the cart, then use the total quantity to validate
        var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
        var addToCartWarnings = await _shoppingCartService
            .GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), cartType,
            product, (await _storeContext.GetCurrentStoreAsync()).Id, string.Empty,
            decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //let's display standard warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //now let's try adding product to the cart (now including product attribute validation, etc)
        addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: await _workContext.GetCurrentCustomerAsync(),
            product: product,
            shoppingCartType: cartType,
            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            attributesXml: attXml,
            quantity: quantity);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //but we do not display attribute and gift card warnings here. let's do it on the product details page
            return Json(new
            {
                redirect = _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) }).Result
            });
        }

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
                        : string.Empty;

                    if (!_opcSettings.BypassShoppingCartPage)
                    {
                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml,
                            redirectbuynow = Url.RouteUrl("ShoppingCart")
                        });
                    }
                    return Json(new
                    {
                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml,
                        updateflyoutcartsectionhtml,
                        redirectbuynow = Url.RouteUrl("Opc")
                    });
                }
        }
    }

    //add product to cart using AJAX
    //currently we use this method on the product details pages
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> AddProductToCartCheckout_Details(int productId, int shoppingCartTypeId, IFormCollection form)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("Homepage")
            });
        }

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                success = false,
                message = "Only simple products could be added to the cart"
            });
        }

        //update existing shopping cart item
        var updatecartitemid = 0;
        foreach (var formKey in form.Keys)
            if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
            {
                int.TryParse(form[formKey], out updatecartitemid);
                break;
            }

        ShoppingCartItem updatecartitem = null;
        if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
        {
            //search with the same cart type as specified
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, (await _storeContext.GetCurrentStoreAsync()).Id);

            updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
            //not found? let's ignore it. in this case we'll add a new item
            //if (updatecartitem == null)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        message = "No shopping cart item found to update"
            //    });
            //}
            //is it this product?
            if (updatecartitem != null && product.Id != updatecartitem.ProductId)
            {
                return Json(new
                {
                    success = false,
                    message = "This product does not match a passed shopping cart item identifier"
                });
            }
        }

        var addToCartWarnings = new List<string>();

        //customer entered price
        var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //entered quantity
        var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

        //product and gift card attributes
        var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
            //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
            updatecartitem.ShoppingCartType;

        await SaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

        //return result
        return await GetProductToCartDetailsAsync(addToCartWarnings, cartType, product);
    }

    #endregion Buy Now
    #endregion Methods
}