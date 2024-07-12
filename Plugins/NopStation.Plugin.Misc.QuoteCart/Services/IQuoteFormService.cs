using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IQuoteFormService
{
    Task DeleteQuoteFormAsync(QuoteForm form);

    Task<IList<QuoteForm>> GetActiveFormsAsync();

    Task<IPagedList<QuoteForm>> GetAllQuoteFormsAsync(int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<QuoteForm> GetFormByIdAsync(int formId);

    Task<IPagedList<QuoteForm>> GetQuoteFormsByFormAttributeIdAsync(int formAttributeId, bool includeDeleted = false, int pageIndex = 0, int pageSize = int.MaxValue);

    Task InsertQuoteFormAsync(QuoteForm form);

    Task UpdateQuoteFormAsync(QuoteForm form);
}