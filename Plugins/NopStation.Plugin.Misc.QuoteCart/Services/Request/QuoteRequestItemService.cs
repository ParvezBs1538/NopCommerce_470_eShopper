using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public class QuoteRequestItemService : IQuoteRequestItemService
{
    #region Fields

    private readonly IRepository<QuoteRequestItem> _quoteRequestItemRepository;

    #endregion

    #region Ctor

    public QuoteRequestItemService(IRepository<QuoteRequestItem> quoteRequestRepository)
    {
        _quoteRequestItemRepository = quoteRequestRepository;
    }

    #endregion

    #region Methods

    public async Task<IPagedList<QuoteRequestItem>> GetAllQuoteRequestItemsAsync(int requestId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from s in _quoteRequestItemRepository.Table
                    select s;

        if (storeId > 0)
            query = query.Where(s => s.StoreId == storeId);

        if (requestId > 0)
            query = query.Where(s => s.QuoteRequestId == requestId);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<QuoteRequestItem> GetQuoteRequestItemByIdAsync(int quoteId)
    {
        if (quoteId == 0)
            return null;

        return await _quoteRequestItemRepository.GetByIdAsync(quoteId, includeDeleted: false);
    }

    public async Task InsertQuoteRequestItemAsync(QuoteRequestItem quoteItem)
    {
        await _quoteRequestItemRepository.InsertAsync(quoteItem);
    }

    public async Task InsertQuoteRequestItemsAsync(IList<QuoteRequestItem> quoteItems)
    {
        await _quoteRequestItemRepository.InsertAsync(quoteItems);
    }

    public async Task UpdateQuoteRequestItemAsync(QuoteRequestItem quoteItem)
    {
        await _quoteRequestItemRepository.UpdateAsync(quoteItem);
    }

    public async Task DeleteQuoteRequestItemAsync(QuoteRequestItem quoteItem)
    {
        await _quoteRequestItemRepository.DeleteAsync(quoteItem);
    }

    #endregion
}
