using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class QuoteFormService : IQuoteFormService
{
    #region Fields

    private readonly IRepository<FormAttributeMapping> _formAttributeMappingRepository;
    private readonly IRepository<QuoteForm> _quoteFormRepository;
    private readonly IStoreContext _storeContext;
    private readonly IStoreMappingService _storeMappingService;

    #endregion

    #region Ctor

    public QuoteFormService(
        IRepository<FormAttributeMapping> formAttributeMappingRepository,
        IRepository<QuoteForm> quoteFormRepository,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService)
    {
        _formAttributeMappingRepository = formAttributeMappingRepository;
        _quoteFormRepository = quoteFormRepository;
        _storeContext = storeContext;
        _storeMappingService = storeMappingService;
    }

    #endregion

    #region Methods

    #region Quote form

    public async Task<IPagedList<QuoteForm>> GetAllQuoteFormsAsync(
        int storeId = 0,
        bool? active = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = from s in _quoteFormRepository.Table
                    where (!active.HasValue || s.Active == active.Value) && s.Deleted == false
                    select s;

        query = await _storeMappingService.ApplyStoreMapping(query, storeId);
        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<IList<QuoteForm>> GetActiveFormsAsync()
    {
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();

        return await GetAllQuoteFormsAsync(storeId, true);
    }


    public async Task<QuoteForm> GetFormByIdAsync(int formId)
    {
        return await _quoteFormRepository.GetByIdAsync(formId, x => default);
    }

    public async Task InsertQuoteFormAsync(QuoteForm form)
    {
        await _quoteFormRepository.InsertAsync(form);
    }

    public async Task UpdateQuoteFormAsync(QuoteForm form)
    {
        await _quoteFormRepository.UpdateAsync(form);
    }

    public async Task DeleteQuoteFormAsync(QuoteForm form)
    {
        await _quoteFormRepository.DeleteAsync(form);
    }

    #endregion

    #region Form attribute

    public async Task<IPagedList<QuoteForm>> GetQuoteFormsByFormAttributeIdAsync(
        int formAttributeId,
        bool includeDeleted = false,
        int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var query = from f in _quoteFormRepository.Table
                    join fam in _formAttributeMappingRepository.Table on f.Id equals fam.QuoteFormId
                    where
                        fam.FormAttributeId == formAttributeId &&
                        (!f.Deleted || includeDeleted)
                    orderby f.DisplayOrder, f.Id
                    select f;

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #endregion
}
