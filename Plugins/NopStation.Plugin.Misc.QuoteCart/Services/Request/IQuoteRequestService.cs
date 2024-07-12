using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public interface IQuoteRequestService
{
    Task<IPagedList<QuoteRequest>> GetAllQuoteRequestsAsync(List<int> requestStatus = null, string customerEmail = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, int formId = 0, DateTime? dateStart = null, DateTime? dateEnd = null, bool includeDeleted = false);

    Task<QuoteRequest> GetQuoteRequestByIdAsync(int requestId);

    Task<QuoteRequest> GetQuoteRequestByGuidAsync(Guid requestId);

    Task InsertQuoteRequestAsync(QuoteRequest request);

    Task UpdateQuoteRequestAsync(QuoteRequest request);

    Task DeleteQuoteRequestAsync(QuoteRequest request);

    Task<IList<QuoteRequestItem>> GetItemsByQuoteRequestId(int quoteRequestId);

    IList<QuoteRequestItem> ParseQuoteRequestItems(string requestItemsXml);

    Task RestoreOriginalAsync(QuoteRequest quoteRequest);

    Task<IPagedList<QuoteRequest>> GetAllQuoteRequestsByCustomerIdAsync(int customerId, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IList<FormSubmissionAttribute>> GetFormSubmissionAttributesByQuoteRequestIdAsync(int quoteRequestId);

    Task<FormSubmissionAttribute> GetFormSubmissionAttributeByIdAsync(int formSubmissionAttributeId);

    Task UpdateFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute);

    Task InsertFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute);

    Task DeleteFormSubmissionAttributeAsync(FormSubmissionAttribute formSubmissionAttribute);

    Task<(decimal subTotalWithTax, decimal subTotalWithoutTax)> GetRequestSubTotalAsync(QuoteRequest quoteRequest);

    Task<(decimal priceWithTax, decimal priceWithoutTax)> GetRequestItemPriceAsync(QuoteRequestItem quoteRequestItem, Customer customer, int quantity = 1);

    ShoppingCartItem MapShoppingCartItem(QuoteRequestItem quoteRequestItem);

    Task<string> GetCustomerEmailAsync(QuoteRequest quoteRequest, bool addGuestSuffix = false);

    Task<Currency> GetCustomerCurrencyAsync(Customer customer, int storeId = 0);
}
