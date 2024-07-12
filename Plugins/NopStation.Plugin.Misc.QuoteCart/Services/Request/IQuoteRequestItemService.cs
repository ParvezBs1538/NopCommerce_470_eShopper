
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public interface IQuoteRequestItemService
{
    Task<IPagedList<QuoteRequestItem>> GetAllQuoteRequestItemsAsync(int requestId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<QuoteRequestItem> GetQuoteRequestItemByIdAsync(int quoteId);

    Task InsertQuoteRequestItemAsync(QuoteRequestItem quoteItem);

    Task UpdateQuoteRequestItemAsync(QuoteRequestItem quoteItem);

    Task DeleteQuoteRequestItemAsync(QuoteRequestItem quoteItem);

    Task InsertQuoteRequestItemsAsync(IList<QuoteRequestItem> quoteItems);
}