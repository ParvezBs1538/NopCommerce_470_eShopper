using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public class QuoteRequestService : IQuoteRequestService
{
    #region Fields 

    private readonly IRepository<QuoteRequest> _quoteRequestRepository;
    private readonly IRepository<FormSubmissionAttribute> _formSubmissionAttributeRepository;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly ITaxService _taxService;
    private readonly IWorkContext _workContext;
    private readonly ILocalizationService _localizationService;
    private readonly IRepository<QuoteRequestItem> _quoteRequestItemsRepository;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly ILanguageService _languageService;

    #endregion

    #region Ctor 

    public QuoteRequestService(
        IRepository<QuoteRequest> quoteRequestRepository,
        IRepository<FormSubmissionAttribute> formSubmissionAttributeRepository,
        IShoppingCartService shoppingCartService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IProductService productService,
        ITaxService taxService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IRepository<QuoteRequestItem> quoteRequestItemsRepository,
        IStoreContext storeContext,
        IStoreService storeService,
        ILanguageService languageService)
    {
        _quoteRequestRepository = quoteRequestRepository;
        _formSubmissionAttributeRepository = formSubmissionAttributeRepository;
        _shoppingCartService = shoppingCartService;
        _currencyService = currencyService;
        _customerService = customerService;
        _productService = productService;
        _taxService = taxService;
        _workContext = workContext;
        _localizationService = localizationService;
        _quoteRequestItemsRepository = quoteRequestItemsRepository;
        _storeContext = storeContext;
        _storeService = storeService;
        _languageService = languageService;
    }

    #endregion

    #region Methods

    #region Quote request

    public async Task<string> GetCustomerEmailAsync(QuoteRequest quoteRequest, bool addGuestSuffix = false)
    {
        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

        return customer != null && !await _customerService.IsGuestAsync(customer) ? customer.Email :
            addGuestSuffix ? string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.FormattedGuestEmail"), quoteRequest.GuestEmail) : quoteRequest.GuestEmail;
    }

    public async Task<IPagedList<QuoteRequest>> GetAllQuoteRequestsAsync(
        List<int> requestStatus = null,
        string customerEmail = null,
        int storeId = 0,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        int formId = 0,
        DateTime? dateStart = null,
        DateTime? dateEnd = null, bool includeDeleted = false)
    {
        var query = from s in _quoteRequestRepository.Table
                    select s;

        if (!includeDeleted)
            query = query.Where(r => !r.Deleted);

        query = query.OrderByDescending(s => s.Id);

        if (storeId > 0)
            query = query.Where(s => s.StoreId == storeId);

        if (formId > 0)
            query = query.Where(s => s.FormId == formId);

        if (requestStatus?.Any(x => x != 0) ?? false)
            query = query.Where(q => requestStatus.Contains(q.RequestStatusId));

        if (dateStart.HasValue)
            query = query.Where(x => x.CreatedOnUtc.Date >= dateStart.Value.Date);

        if (dateEnd.HasValue)
            query = query.Where(x => x.CreatedOnUtc.Date <= dateEnd.Value.Date);

        if (!string.IsNullOrEmpty(customerEmail))
        {
            var customer = await _customerService.GetCustomerByEmailAsync(customerEmail);
            if (customer == null)
                query = null;
            else
                query = query.Where(s => s.CustomerId == customer.Id);
        }

        return await query.OrderByDescending(x => x.CreatedOnUtc).ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<QuoteRequest> GetQuoteRequestByIdAsync(int quoteId)
    {
        if (quoteId == 0)
            return null;

        return await _quoteRequestRepository.GetByIdAsync(quoteId, includeDeleted: false);
    }

    public async Task<QuoteRequest> GetQuoteRequestByGuidAsync(Guid requestId)
    {
        return await _quoteRequestRepository.Table.FirstAsync(s => s.RequestId == requestId);
    }

    public async Task InsertQuoteRequestAsync(QuoteRequest request)
    {
        await _quoteRequestRepository.InsertAsync(request);
    }

    public async Task UpdateQuoteRequestAsync(QuoteRequest request)
    {
        await _quoteRequestRepository.UpdateAsync(request);
    }

    public async Task DeleteQuoteRequestAsync(QuoteRequest request)
    {
        await _quoteRequestRepository.DeleteAsync(request);
    }

    public async Task<IList<QuoteRequestItem>> GetItemsByQuoteRequestId(int quoteRequestId)
    {
        return await _quoteRequestItemsRepository.Table
            .Where(x => x.QuoteRequestId == quoteRequestId)
            .ToListAsync();
    }

    public async Task<IPagedList<QuoteRequest>> GetAllQuoteRequestsByCustomerIdAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _quoteRequestRepository.Table
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task RestoreOriginalAsync(QuoteRequest quoteRequest)
    {
        await _quoteRequestItemsRepository.DeleteAsync(x => x.QuoteRequestId == quoteRequest.Id);

        var requestItems = ParseQuoteRequestItems(quoteRequest.OriginalRequestItemsXml);

        foreach (var item in requestItems)
        {
            item.QuoteRequestId = quoteRequest.Id;
        }

        await _quoteRequestItemsRepository.InsertAsync(requestItems);
    }

    public IList<QuoteRequestItem> ParseQuoteRequestItems(string requestItemsXml)
    {
        var items = new List<QuoteRequestItem>();

        if (string.IsNullOrEmpty(requestItemsXml))
            return items;

        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(requestItemsXml);

            foreach (XmlNode xmlNode in xmlDoc.SelectNodes(@"//QuoteItems//QuoteItem"))
            {
                var qri = new QuoteRequestItem();

                foreach (var childNode in xmlNode.ChildNodes)
                {
                    if (childNode is not XmlElement element)
                        continue;

                    var data = element.InnerText;
                    object _ = element.Name switch
                    {
                        nameof(qri.ProductId) => qri.ProductId = int.Parse(data),
                        nameof(qri.Quantity) => qri.Quantity = int.Parse(data),
                        nameof(qri.CustomerId) => qri.CustomerId = int.Parse(data),
                        nameof(qri.StoreId) => qri.StoreId = int.Parse(data),
                        nameof(qri.QuoteRequestId) => qri.QuoteRequestId = int.Parse(data),
                        nameof(qri.DiscountedPrice) => qri.DiscountedPrice = decimal.Parse(data),
                        nameof(qri.CreatedOnUtc) => qri.CreatedOnUtc = DateTime.Parse(data),
                        nameof(qri.UpdatedOnUtc) => qri.UpdatedOnUtc = DateTime.Parse(data),
                        nameof(qri.AttributesXml) => qri.AttributesXml = data,
                        _ => null,
                    };
                }

                items.Add(qri);
            }
        }
        catch (Exception exc)
        {
            Debug.Write(exc.ToString());
        }

        return items;
    }

    #endregion

    #region Form submission attribute

    public async Task<IList<FormSubmissionAttribute>> GetFormSubmissionAttributesByQuoteRequestIdAsync(int quoteRequestId)
    {
        var query = from s in _formSubmissionAttributeRepository.Table
                    where s.QuoteRequestId == quoteRequestId
                    select s;

        return await query.ToListAsync();
    }

    public async Task<FormSubmissionAttribute> GetFormSubmissionAttributeByIdAsync(int formSubmissionAttributeId)
    {
        return await _formSubmissionAttributeRepository.GetByIdAsync(formSubmissionAttributeId);
    }

    //update form submission attribute
    public async Task UpdateFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute)
    {
        await _formSubmissionAttributeRepository.UpdateAsync(formSubmissionAttribute);
    }

    //insert form submission attribute
    public async Task InsertFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute)
    {
        await _formSubmissionAttributeRepository.InsertAsync(formSubmissionAttribute);
    }

    //delete form submission attribute
    public async Task DeleteFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute)
    {
        await _formSubmissionAttributeRepository.DeleteAsync(formSubmissionAttribute);
    }

    #endregion

    #region Utilities

    public ShoppingCartItem MapShoppingCartItem(QuoteRequestItem quoteRequestItem)
    {
        return new ShoppingCartItem
        {
            AttributesXml = quoteRequestItem.AttributesXml,
            CreatedOnUtc = quoteRequestItem.CreatedOnUtc,
            CustomerId = quoteRequestItem.CustomerId,
            ProductId = quoteRequestItem.ProductId,
            Quantity = quoteRequestItem.Quantity,
            StoreId = quoteRequestItem.StoreId,
            UpdatedOnUtc = quoteRequestItem.UpdatedOnUtc
        };
    }

    public async Task<(decimal subTotalWithTax, decimal subTotalWithoutTax)> GetRequestSubTotalAsync(QuoteRequest quoteRequest)
    {
        var subTotalWithoutTax = decimal.Zero;
        var subTotalWithTax = decimal.Zero;

        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

        var quoteRequestItems = await _quoteRequestItemsRepository.GetAllAsync(items => items.Where(x => x.QuoteRequestId == quoteRequest.Id));

        foreach (var item in quoteRequestItems)
        {
            var (priceWithTax, priceWithoutTax) = await GetRequestItemPriceAsync(item, customer, item.Quantity);

            subTotalWithTax += priceWithTax;
            subTotalWithoutTax += priceWithoutTax;
        }

        return (subTotalWithTax, subTotalWithoutTax);
    }

    public async Task<(decimal priceWithTax, decimal priceWithoutTax)> GetRequestItemPriceAsync(QuoteRequestItem quoteRequestItem, Customer customer, int quantity = 1)
    {
        var product = await _productService.GetProductByIdAsync(quoteRequestItem.ProductId) ?? throw new NopException($"Product with ID {quoteRequestItem.ProductId} not found.");

        var unitPrice = quoteRequestItem.DiscountedPrice > 0 ? quoteRequestItem.DiscountedPrice : product.Price;

        var sci = new ShoppingCartItem
        {
            AttributesXml = quoteRequestItem.AttributesXml,
            StoreId = quoteRequestItem.StoreId,
            ProductId = quoteRequestItem.ProductId,
            Quantity = quoteRequestItem.Quantity,
            ShoppingCartType = ShoppingCartType.ShoppingCart,
            CustomerId = customer?.Id ?? 0,
        };

        if (customer == null || quoteRequestItem.DiscountedPrice > 0)
        {
            return (unitPrice * quantity, unitPrice * quantity);
        }
        //unit prices
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        if (product.CallForPrice)
        {
            return (0, 0);
        }
        else
        {
            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, false)).unitPrice);
            unitPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
        }

        var (priceWithoutTax, _) = await _taxService.GetProductPriceAsync(product, unitPrice * quantity, false, customer);
        var (priceWithTax, _) = await _taxService.GetProductPriceAsync(product, unitPrice * quantity, true, customer);

        return (priceWithTax, priceWithoutTax);
    }

    public async Task<Currency> GetCustomerCurrencyAsync(Customer customer, int storeId = 0)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync();
        var allStoreCurrencies = await _currencyService.GetAllCurrenciesAsync(storeId: store.Id);
        var customerCurrency = allStoreCurrencies.FirstOrDefault(currency => currency.Id == customer.CurrencyId);
        if (customerCurrency == null)
        {
            //it not found, then try to get the default currency for the current language (if specified)
            var language = await _languageService.GetLanguageByIdAsync(customer.LanguageId ?? store.DefaultLanguageId);
            if (language != null)
            {
                customerCurrency = allStoreCurrencies
                    .FirstOrDefault(currency => currency.Id == language.DefaultCurrencyId);
            }
        }

        return customerCurrency ?? allStoreCurrencies.FirstOrDefault();
    }

    #endregion

    #endregion
}
