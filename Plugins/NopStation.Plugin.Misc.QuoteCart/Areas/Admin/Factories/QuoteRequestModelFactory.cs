using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;
using NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public partial class QuoteRequestModelFactory : IQuoteRequestModelFactory
{
    #region Fields

    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly ICountryService _countryService;
    private readonly ICurrencyService _currencyService;
    private readonly CatalogSettings _catalogSettings;
    private readonly CurrencySettings _currencySettings;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IAddressService _addressService;
    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IShippingPluginManager _shippingPluginManager;
    private readonly IShippingService _shippingService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly IStoreService _storeService;
    private readonly ITaxService _taxService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
    private readonly ICustomerService _customerService;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IFormAttributeParser _formAttributeParser;
    private readonly IFormAttributeService _formAttributeService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHtmlHelper _htmlHelper;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductService _productService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly ILocalizationService _localizationService;
    private readonly IQuoteRequestMessageService _quoteRequestMessageService;

    #endregion

    #region Ctor

    public QuoteRequestModelFactory(
        CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        IActionContextAccessor actionContextAccessor,
        IAddressService addressService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        ICustomerModelFactory customerModelFactory,
        IFormAttributeParser formAttributeParser,
        IFormAttributeService formAttributeService,
        IGenericAttributeService genericAttributeService,
        IHtmlHelper htmlHelper,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IPaymentPluginManager paymentPluginManager,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IQuoteFormService quoteFormService,
        IQuoteRequestItemService quoteRequestItemService,
        IQuoteRequestMessageService quoteRequestMessageService,
        IQuoteRequestService quoteRequestService,
        IShippingPluginManager shippingPluginManager,
        IShippingService shippingService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
        IStoreService storeService,
        ITaxService taxService,
        IUrlHelperFactory urlHelperFactory,
        IWorkContext workContext)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _countryService = countryService;
        _currencyService = currencyService;
        _catalogSettings = catalogSettings;
        _currencySettings = currencySettings;
        _actionContextAccessor = actionContextAccessor;
        _addressService = addressService;
        _quoteRequestService = quoteRequestService;
        _shippingPluginManager = shippingPluginManager;
        _shippingService = shippingService;
        _stateProvinceService = stateProvinceService;
        _quoteRequestItemService = quoteRequestItemService;
        _storeService = storeService;
        _taxService = taxService;
        _urlHelperFactory = urlHelperFactory;
        _workContext = workContext;
        _storeContext = storeContext;
        _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        _customerService = customerService;
        _customerModelFactory = customerModelFactory;
        _formAttributeParser = formAttributeParser;
        _formAttributeService = formAttributeService;
        _genericAttributeService = genericAttributeService;
        _htmlHelper = htmlHelper;
        _messageTemplateService = messageTemplateService;
        _paymentPluginManager = paymentPluginManager;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _quoteFormService = quoteFormService;
        _localizationService = localizationService;
        _quoteRequestMessageService = quoteRequestMessageService;
    }

    #endregion

    #region Methods

    #region Quote request

    public virtual async Task<QuoteRequestSearchModel> PrepareQuoteRequestSearchModelAsync(QuoteRequestSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare available request statuses
        searchModel.AvailableRequestStatuses = _htmlHelper.GetEnumSelectList(typeof(RequestStatus)).ToList();

        if (searchModel.AvailableRequestStatuses.Any())
        {
            if (searchModel.SearchRequestStatusIds?.Count > 0)
            {
                var ids = searchModel.SearchRequestStatusIds.Select(id => id.ToString());
                searchModel.AvailableRequestStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                    .ForEach(statusItem => statusItem.Selected = true);
            }
            else if (searchModel.AvailableRequestStatuses.FirstOrDefault() is SelectListItem selectedItem)
                selectedItem.Selected = true;
        }

        //prepare available stores
        await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

        var forms = await _quoteFormService.GetAllQuoteFormsAsync(searchModel.SearchStoreId);
        searchModel.AvailableForms = forms
            .Select(x => new SelectListItem(x.Title, x.Id.ToString(), searchModel.SearchFormId == x.Id)).ToList();

        searchModel.AvailableForms
            .Insert(0, new (await _localizationService.GetResourceAsync("Admin.Common.All"),"0"));

        //prepare grid
        searchModel.SetGridPageSize();

        searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

        return searchModel;
    }

    public virtual async Task<QuoteRequestListModel> PrepareQuoteRequestListModelAsync(QuoteRequestSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get requests
        var quoteRequests = await _quoteRequestService.GetAllQuoteRequestsAsync(
            customerEmail: searchModel.SearchCustomerEmail,
            dateStart: searchModel.SearchStartDate,
            dateEnd: searchModel.SearchEndDate,
            requestStatus: searchModel.SearchRequestStatusIds,
            formId: searchModel.SearchFormId,
            storeId: searchModel.SearchStoreId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        //prepare list model
        var model = await new QuoteRequestListModel().PrepareToGridAsync(searchModel, quoteRequests, () =>
        {
            return quoteRequests.SelectAwait(async request =>
            {
                var quoteRequest = request.ToModel<QuoteRequestModel>();
                quoteRequest.CreatedOn = request.CreatedOnUtc;
                var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);
                quoteRequest.CustomerEmail = await _quoteRequestService.GetCustomerEmailAsync(request, true);
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                quoteRequest.ShareQuote = $"{_storeContext.GetCurrentStore().Url.TrimEnd('/')}{urlHelper.RouteUrl("QuoteCart.RequestDetails", new { requestId = quoteRequest.RequestId })}";
                return quoteRequest;
            });
        });

        return model;
    }

    public virtual async Task<QuoteRequestDetailsModel> PrepareQuoteRequestModelAsync(QuoteRequestDetailsModel model, QuoteRequest quoteRequest, bool excludeProperties = false)
    {
        if (quoteRequest != null)
        {
            model ??= new QuoteRequestDetailsModel()
            {
                Id = quoteRequest.Id,
                RequestStatusId = quoteRequest.RequestStatusId,
                CreatedOnUtc = quoteRequest.CreatedOnUtc,
                UpdatedOnUtc = quoteRequest.UpdatedOnUtc
            };
            model.RequestStatus = await _localizationService.GetLocalizedEnumAsync(quoteRequest.RequestStatus);

            var store = await _storeService.GetStoreByIdAsync(quoteRequest.StoreId);
            model.StoreName = store.Name;

            var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            model.CustomerModel = await _customerModelFactory.PrepareCustomerModelAsync(new(), customer);
            model.CustomerModel.Email = await _quoteRequestService.GetCustomerEmailAsync(quoteRequest);
            model.CustomerModel.FullName = customer != null && !await _customerService.IsGuestAsync(customer) ? await _customerService.GetCustomerFullNameAsync(customer) : await _localizationService.GetResourceAsync("Customer.Guest");
            model.ShareQuote = $"{_storeContext.GetCurrentStore().Url}QuoteRequest/CustomerRequestDetails?requestId={quoteRequest.RequestId}";
            var messages = await _quoteRequestMessageService.GetAllQuoteRequestMessagesAsync(requestId: quoteRequest.Id, storeId: store.Id);

            model.RequestMessages = messages.Select(x =>
            {
                var messageModel = x.ToModel<QuoteRequestMessageModel>();
                messageModel.IsWriter = x.CustomerId == currentCustomer.Id;
                return messageModel;
            }).ToList();

            var quoteRequestItems = await _quoteRequestItemService.GetAllQuoteRequestItemsAsync(requestId: quoteRequest.Id);

            model.QuoteRequestItems = await quoteRequestItems.SelectAwait(async x =>
            {
                return await PrepareQuoteRequestItemModelAsync(null, x, quoteRequest);
            }).ToListAsync();

            var (subTotalWithTax, subTotalWithoutTax) = await _quoteRequestService.GetRequestSubTotalAsync(quoteRequest);
            model.SubTotalStr = await _priceFormatter.FormatPriceAsync(subTotalWithoutTax);

            var quoteForm = await _quoteFormService.GetFormByIdAsync(quoteRequest.FormId);
            model.QuoteFormName = await _localizationService.GetLocalizedAsync(quoteForm, x => x.Title);

            model.SubmittedFormAttributes = await PrepareSubmittedFormAttributeModelsAsync(quoteRequest);
            model.RequestGuid = quoteRequest.RequestId.ToString();

            model.CanCancelRequest = quoteRequest.RequestStatus == RequestStatus.Pending;
        }
        return model;
    }

    public virtual async Task<MessageTemplateListModel> PrepareMessageTemplateListModelAsync(MessageTemplateSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get message templates
        var messageTemplates = (await _messageTemplateService
            .GetAllMessageTemplatesAsync(searchModel.SearchStoreId)).Where(x => x.Name.StartsWith(QuoteCartDefaults.SYSTEM_NAME)).ToList().ToPagedList(searchModel);

        //prepare store names (to avoid loading for each message template)
        var stores = (await _storeService.GetAllStoresAsync()).Select(store => new { store.Id, store.Name }).ToList();

        //prepare list model
        var model = await new MessageTemplateListModel().PrepareToGridAsync(searchModel, messageTemplates, () =>
        {
            return messageTemplates.SelectAwait(async messageTemplate =>
            {
                //fill in model values from the entity
                var messageTemplateModel = messageTemplate.ToModel<MessageTemplateModel>();

                //fill in additional values (not existing in the entity)
                var storeNames = stores.Select(store => store.Name);
                if (messageTemplate.LimitedToStores)
                {
                    await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(messageTemplateModel, messageTemplate, false);
                    storeNames = stores
                        .Where(store => messageTemplateModel.SelectedStoreIds.Contains(store.Id)).Select(store => store.Name);
                }

                messageTemplateModel.ListOfStores = string.Join(", ", storeNames);

                return messageTemplateModel;
            });
        });

        return model;
    }

    #endregion

    #region Quote request item

    public async Task<QuoteRequestItemModel> PrepareQuoteRequestItemModelAsync(QuoteRequestItemModel model, QuoteRequestItem quoteRequestItem, QuoteRequest quoteRequest, bool excludeProperties = true)
    {
        var customer = await _customerService.GetCustomerByIdAsync(quoteRequestItem?.CustomerId ?? quoteRequest?.CustomerId ?? 0) ?? await _workContext.GetCurrentCustomerAsync();

        if (quoteRequestItem != null)
        {
            model ??= quoteRequestItem.ToModel<QuoteRequestItemModel>();
            var (_, discountedPrice) = await _quoteRequestService.GetRequestItemPriceAsync(quoteRequestItem, customer, 1);
            model.DiscountedPriceStr = await _priceFormatter.FormatPriceAsync(discountedPrice);
            var (_, totalPrice) = await _quoteRequestService.GetRequestItemPriceAsync(quoteRequestItem, customer, quoteRequestItem.Quantity);
            model.TotalPriceStr = await _priceFormatter.FormatPriceAsync(totalPrice);

        }

        if (model?.ProductId > 0)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            model.ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            model.ProductPrice = product.Price;
            model.ProductSku = product.Sku;
            if (model.DiscountedPrice <= 0)
                model.DiscountedPrice = model.ProductPrice;
            model.FormattedAttributes = quoteRequestItem == null ? string.Empty : await _productAttributeFormatter.FormatAttributesAsync(product, quoteRequestItem.AttributesXml);

            if (!excludeProperties)
            {
                await PrepareProductAttributeModelsAsync(model.ProductAttributes, quoteRequest, product);
            }
            if (product.IsRental && quoteRequestItem != null)
            {
                model.RentalInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    quoteRequestItem.RentalStartDateUtc, quoteRequestItem.RentalEndDateUtc);
            }
        }

        if (quoteRequest != null)
        {
            model ??= new QuoteRequestItemModel();
            model.QuoteRequestId = quoteRequest.Id;
        }

        model.ProductPriceStr = await _priceFormatter.FormatPriceAsync(model.ProductPrice);

        return model;
    }

    protected virtual async Task PrepareProductAttributeModelsAsync(IList<QuoteRequestItemModel.ProductAttributeModel> models, QuoteRequest quoteRequest, Product product)
    {
        ArgumentNullException.ThrowIfNull(models);

        ArgumentNullException.ThrowIfNull(quoteRequest);

        ArgumentNullException.ThrowIfNull(product);

        var attributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        var store = await _storeService.GetStoreByIdAsync(quoteRequest.StoreId);

        foreach (var attribute in attributes)
        {
            var attributeModel = new QuoteRequestItemModel.ProductAttributeModel
            {
                Id = attribute.Id,
                ProductAttributeId = attribute.ProductAttributeId,
                Name = (await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId)).Name,
                TextPrompt = attribute.TextPrompt,
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
                HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
            };
            if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
            {
                attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            if (attribute.ShouldHaveValues())
            {
                var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

                //values
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var attributeValue in attributeValues)
                {
                    //price adjustment
                    var (priceAdjustment, _) = await _taxService.GetProductPriceAsync(product,
                        await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store));

                    var priceAdjustmentStr = string.Empty;
                    if (priceAdjustment != 0)
                    {
                        if (attributeValue.PriceAdjustmentUsePercentage)
                        {
                            priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                            priceAdjustmentStr = priceAdjustment > 0 ? $"+{priceAdjustmentStr}%" : $"{priceAdjustmentStr}%";
                        }
                        else
                        {
                            priceAdjustmentStr = priceAdjustment > 0 ? $"+{await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false)}" : $"-{await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false)}";
                        }
                    }

                    attributeModel.Values.Add(new QuoteRequestItemModel.ProductAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = attributeValue.Name,
                        IsPreSelected = attributeValue.IsPreSelected,
                        CustomerEntersQty = attributeValue.CustomerEntersQty,
                        Quantity = attributeValue.Quantity,
                        PriceAdjustment = priceAdjustmentStr,
                        PriceAdjustmentValue = priceAdjustment
                    });
                }
            }

            models.Add(attributeModel);
        }
    }

    #endregion

    #region Convert to order

    public async Task<ConvertToOrderModel> PrepareConvertToOrderModelAsync(ConvertToOrderModel model, QuoteRequest quoteRequest)
    {
        ArgumentNullException.ThrowIfNull(model);

        ArgumentNullException.ThrowIfNull(quoteRequest);

        model.QuoteRequestId = quoteRequest.Id;

        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

        model.QuoteRequestDetails = await PrepareQuoteRequestModelAsync(new QuoteRequestDetailsModel(), quoteRequest);
        model.CustomerModel = await _customerModelFactory.PrepareCustomerModelAsync(new(), customer);
        await _customerModelFactory.PrepareCustomerModelAsync(model.CustomerModel, customer);
        var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
        model.BillingAddressId = billingAddress?.Id ?? 0;
        var shippingAddress = await _addressService.GetAddressByIdAsync(customer.ShippingAddressId ?? 0);
        model.ShippingAddressId = shippingAddress?.Id ?? 0;


        //customer currency
        var currencyTmp = await _quoteRequestService.GetCustomerCurrencyAsync(customer, quoteRequest.StoreId);
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : currentCurrency;
        var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
        model.CustomerCurrencyCode = customerCurrency.CurrencyCode;
        model.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

        await PrepareShippingProvidersAsync(model.AvailableShippingProviders, quoteRequest);

        model.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, quoteRequest.StoreId);
        var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, quoteRequest.StoreId);
        model.ShippingRateComputationMethodSystemName = shippingOption?.ShippingRateComputationMethodSystemName ?? model.AvailableShippingProviders.FirstOrDefault()?.Value;
        model.ShippingMethodId = shippingOption?.Name;
        await PrepareAddressModelsAsync(model.AvailableAddresses, customer);
        await PrepareShippingMethodsAsync(model.AvailableShippingMethods, quoteRequest, model.ShippingRateComputationMethodSystemName);
        await PreparePaymentMethodsAsync(model.AvailablePaymentMethods, customer);

        return model;
    }

    public async Task PrepareShippingMethodsAsync(IList<SelectListItem> items, QuoteRequest quoteRequest, string providerSystemName = "", int shippingAddressId = 0, bool addDefaultItem = false)
    {
        if (string.IsNullOrEmpty(providerSystemName))
            return;

        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

        var provider = (await _shippingPluginManager.LoadActivePluginsAsync(customer, quoteRequest.StoreId, providerSystemName)).FirstOrDefault();

        if (provider == null)
            return;

        var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();

        foreach (var shippingMethod in shippingMethods)
        {
            var methodName = shippingMethod.Name;
            if (!string.IsNullOrEmpty(shippingMethod.Description))
                methodName += " (" + shippingMethod.Description + ")";
            items.Add(new (methodName, shippingMethod.Name));
        }

        if (addDefaultItem)
        {
            items.Insert(0, new (await _localizationService.GetResourceAsync("Admin.Common.Select"), "0"));
        }
    }

    protected async Task PrepareShippingProvidersAsync(IList<SelectListItem> items, QuoteRequest quoteRequest, bool addDefaultItem = false)
    {
        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);
        var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(customer, quoteRequest.StoreId);

        foreach (var provider in shippingProviders)
        {
            items.Add(new (provider.PluginDescriptor.FriendlyName, provider.PluginDescriptor.SystemName));
        }

        if (addDefaultItem)
        {
            items.Insert(0, new (await _localizationService.GetResourceAsync("Admin.Common.Select"), "0"));
        }
    }

    protected async Task PreparePaymentMethodsAsync(IList<SelectListItem> items, Customer customer, bool addDefaultItem = false)
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var paymentMethods = await _paymentPluginManager.LoadActivePluginsAsync(storeId: storeId, customer: customer);

        foreach (var paymentMethod in paymentMethods)
        {
            items.Add(new (paymentMethod.PluginDescriptor.FriendlyName, paymentMethod.PluginDescriptor.SystemName));
        }

        if (addDefaultItem)
        {
            items.Insert(0, new (await _localizationService.GetResourceAsync("Admin.Common.Select"), "0"));
        }
    }

    protected async Task PrepareAddressModelsAsync(IList<SelectListItem> items, Customer customer, bool addDefaultItem = false)
    {
        var addresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
        foreach (var address in addresses)
        {
            var addressModel = address;
            var addressLine = "";
            addressLine += address.FirstName;
            addressLine += " " + address.LastName;
            if (!string.IsNullOrEmpty(addressModel.Address1))
            {
                addressLine += ", " + addressModel.Address1;
            }
            if (!string.IsNullOrEmpty(addressModel.City))
            {
                addressLine += ", " + addressModel.City;
            }
            if (!string.IsNullOrEmpty(addressModel.County))
            {
                addressLine += ", " + addressModel.County;
            }
            if (address.StateProvinceId.HasValue)
            {
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
                if (!string.IsNullOrEmpty(stateProvince?.Name))
                    addressLine += ", " + stateProvince.Name;
            }
            if (!string.IsNullOrEmpty(addressModel.ZipPostalCode))
            {
                addressLine += " " + addressModel.ZipPostalCode;
            }
            if (address.CountryId.HasValue)
            {
                var country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);
                if (!string.IsNullOrEmpty(country?.Name))
                    addressLine += ", " + country.Name;
            }
            items.Add(new SelectListItem
            {
                Text = addressLine,
                Value = address.Id.ToString()
            });
        }

        if (addDefaultItem)
        {
            items.Insert(0, new (await _localizationService.GetResourceAsync("Checkout.NewAddress"), "0"));
        }
    }

    #endregion

    #region Submitted form attribute


    public async Task<IList<SubmittedFormAttributeModel>> PrepareSubmittedFormAttributeModelsAsync(QuoteRequest quoteRequest)
    {
        var model = new List<SubmittedFormAttributeModel>();

        var attributes = await _formAttributeParser.ParseFormAttributeMappingsAsync(quoteRequest.AttributeXml);
        foreach (var attribute in attributes)
        {
            var formAttribute = await _formAttributeService.GetFormAttributeByIdAsync(attribute.FormAttributeId);
            var values = _formAttributeParser.ParseValues(quoteRequest.AttributeXml, attribute.Id);
            var attributeModel = new SubmittedFormAttributeModel
            {
                Name = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(formAttribute, x => x.Description),
                AttributeControlType = attribute.AttributeControlType,
                TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
            };

            if (attribute.AttributeControlType is Domain.AttributeControlType.DropdownList
                or Domain.AttributeControlType.ImageSquares
                or Domain.AttributeControlType.Checkboxes
                or Domain.AttributeControlType.RadioList
                or Domain.AttributeControlType.ReadonlyCheckboxes
                or Domain.AttributeControlType.ColorSquares)
            {
                foreach (var value in values)
                {
                    if (await _formAttributeService.GetFormAttributeValueByIdAsync(int.Parse(value)) is var formAttributeValue)
                        attributeModel.Values.Add(await _localizationService.GetLocalizedAsync(formAttributeValue, x => x.Name));
                }
            }
            else if (attribute.AttributeControlType is Domain.AttributeControlType.FileUpload)
            {

            }
            else
            {
                attributeModel.Values = values;
            }

            model.Add(attributeModel);
        }

        return model;
    }

    #endregion

    #endregion
}
