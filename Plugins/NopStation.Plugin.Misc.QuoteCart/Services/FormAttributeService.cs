using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class FormAttributeService : IFormAttributeService
{
    #region Fields

    private readonly IRepository<FormAttribute> _formAttributeRepository;
    private readonly IRepository<FormAttributeMapping> _formAttributeMappingRepository;
    private readonly IRepository<FormAttributeValue> _formAttributeValueRepository;
    private readonly IRepository<PredefinedFormAttributeValue> _predefinedFormAttributeValueRepository;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public FormAttributeService(
        IRepository<FormAttribute> formAttributeRepository,
        IRepository<FormAttributeMapping> formAttributeMappingRepository,
        IRepository<FormAttributeValue> formAttributeValueRepository,
        IRepository<PredefinedFormAttributeValue> predefinedFormAttributeValueRepository,
        IStaticCacheManager staticCacheManager)
    {
        _formAttributeRepository = formAttributeRepository;
        _formAttributeMappingRepository = formAttributeMappingRepository;
        _formAttributeValueRepository = formAttributeValueRepository;
        _predefinedFormAttributeValueRepository = predefinedFormAttributeValueRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Form attributes

    public virtual async Task DeleteFormAttributeAsync(FormAttribute formAttribute)
    {
        await _formAttributeRepository.DeleteAsync(formAttribute);
    }

    public virtual async Task DeleteFormAttributesAsync(IList<FormAttribute> formAttributes)
    {
        ArgumentNullException.ThrowIfNull(formAttributes);

        foreach (var formAttribute in formAttributes)
            await DeleteFormAttributeAsync(formAttribute);
    }

    public virtual async Task<IPagedList<FormAttribute>> GetAllFormAttributesAsync(int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var formAttributes = await _formAttributeRepository.GetAllPagedAsync(query =>
        {
            return from pa in query
                   orderby pa.Name
                   select pa;
        }, pageIndex, pageSize);

        return formAttributes;
    }

    public virtual async Task<FormAttribute> GetFormAttributeByIdAsync(int formAttributeId)
    {
        return await _formAttributeRepository.GetByIdAsync(formAttributeId, cache => default);
    }

    public virtual async Task<IList<FormAttribute>> GetFormAttributesByIdsAsync(int[] formAttributeIds)
    {
        return await _formAttributeRepository.GetByIdsAsync(formAttributeIds);
    }

    public virtual async Task InsertFormAttributeAsync(FormAttribute formAttribute)
    {
        await _formAttributeRepository.InsertAsync(formAttribute);
    }

    public virtual async Task UpdateFormAttributeAsync(FormAttribute formAttribute)
    {
        await _formAttributeRepository.UpdateAsync(formAttribute);
    }

    public virtual async Task<int[]> GetNotExistingAttributesAsync(int[] attributeId)
    {
        ArgumentNullException.ThrowIfNull(attributeId);

        var query = _formAttributeRepository.Table;
        var queryFilter = attributeId.Distinct().ToArray();
        var filter = await query.Select(a => a.Id)
            .Where(m => queryFilter.Contains(m))
            .ToListAsync();

        return queryFilter.Except(filter).ToArray();
    }

    #endregion

    #region Form attribute mappings

    public async Task<IList<FormAttributeMapping>> GetFormAttributeMappingsByQuoteFormIdAsync(int quoteFormId)
    {
        var allCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.FormAttributeMappingsByQuoteFormCacheKey, quoteFormId);

        var query = from pam in _formAttributeMappingRepository.Table
                    orderby pam.DisplayOrder, pam.Id
                    where pam.QuoteFormId == quoteFormId
                    select pam;

        var attributes = await _staticCacheManager.GetAsync(allCacheKey, async () => await query.ToListAsync()) ?? [];

        return attributes;
    }

    public virtual async Task DeleteFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping)
    {
        await _formAttributeMappingRepository.DeleteAsync(formAttributeMapping);
    }

    public virtual async Task<FormAttributeMapping> GetFormAttributeMappingByIdAsync(int formAttributeMappingId)
    {
        return await _formAttributeMappingRepository.GetByIdAsync(formAttributeMappingId, cache => default);
    }

    public virtual async Task InsertFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping)
    {
        await _formAttributeMappingRepository.InsertAsync(formAttributeMapping);
    }

    public virtual async Task UpdateFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping)
    {
        await _formAttributeMappingRepository.UpdateAsync(formAttributeMapping);
    }

    #endregion

    #region Form Attribute Value

    public virtual async Task DeleteFormAttributeValueAsync(FormAttributeValue formAttributeValue)
    {
        await _formAttributeValueRepository.DeleteAsync(formAttributeValue);
    }

    public virtual async Task<FormAttributeValue> GetFormAttributeValueByIdAsync(int formAttributeValueId)
    {
        return await _formAttributeValueRepository.GetByIdAsync(formAttributeValueId, cache => default);
    }

    public virtual async Task<IList<FormAttributeValue>> GetFormAttributeValuesAsync(int formAttributeMappingId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.FormAttributeValuesByAttributeCacheKey, formAttributeMappingId);

        var query = from pav in _formAttributeValueRepository.Table
                    orderby pav.DisplayOrder, pav.Id
                    where pav.FormAttributeMappingId == formAttributeMappingId
                    select pav;

        var attributes = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync()) ?? [];

        return attributes;
    }

    public virtual async Task InsertFormAttributeValueAsync(FormAttributeValue formAttributeValue)
    {
        await _formAttributeValueRepository.InsertAsync(formAttributeValue);
    }

    public virtual async Task UpdateFormAttributeValueAsync(FormAttributeValue formAttributeValue)
    {
        await _formAttributeValueRepository.UpdateAsync(formAttributeValue);
    }

    #endregion

    #region Predefined form attribute values

    public virtual async Task DeletePredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue)
    {
        await _predefinedFormAttributeValueRepository.DeleteAsync(formAttributeValue);
    }

    public virtual async Task<PredefinedFormAttributeValue> GetPredefinedFormAttributeValueByIdAsync(int formAttributeValueId)
    {
        return await _predefinedFormAttributeValueRepository.GetByIdAsync(formAttributeValueId, cache => default);
    }

    public virtual async Task<IList<PredefinedFormAttributeValue>> GetPredefinedFormAttributeValuesAsync(int formAttributeId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.PredefinedFormAttributeValuesByAttributeCacheKey, formAttributeId);

        var query = from pav in _predefinedFormAttributeValueRepository.Table
                    orderby pav.DisplayOrder, pav.Id
                    where pav.FormAttributeId == formAttributeId
                    select pav;

        var attributes = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync()) ?? [];

        return attributes;
    }

    public virtual async Task InsertPredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue)
    {
        await _predefinedFormAttributeValueRepository.InsertAsync(formAttributeValue);
    }

    public virtual async Task UpdatePredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue)
    {
        await _predefinedFormAttributeValueRepository.UpdateAsync(formAttributeValue);
    }

    #endregion

    #endregion
}
