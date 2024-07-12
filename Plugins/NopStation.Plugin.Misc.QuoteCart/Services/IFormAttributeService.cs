using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IFormAttributeService
{
    Task DeleteFormAttributeAsync(FormAttribute formAttribute);

    Task DeleteFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping);

    Task DeleteFormAttributesAsync(IList<FormAttribute> formAttributes);

    Task DeleteFormAttributeValueAsync(FormAttributeValue formAttributeValue);

    Task DeletePredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue);

    Task<IPagedList<FormAttribute>> GetAllFormAttributesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task<FormAttribute> GetFormAttributeByIdAsync(int formAttributeId);

    Task<FormAttributeMapping> GetFormAttributeMappingByIdAsync(int formAttributeMappingId);

    Task<IList<FormAttributeMapping>> GetFormAttributeMappingsByQuoteFormIdAsync(int quoteFormId);

    Task<IList<FormAttribute>> GetFormAttributesByIdsAsync(int[] attributeIds);

    Task<FormAttributeValue> GetFormAttributeValueByIdAsync(int formAttributeValueId);

    Task<IList<FormAttributeValue>> GetFormAttributeValuesAsync(int formAttributeMappingId);

    Task<int[]> GetNotExistingAttributesAsync(int[] attributeId);

    Task<PredefinedFormAttributeValue> GetPredefinedFormAttributeValueByIdAsync(int formAttributeValueId);

    Task<IList<PredefinedFormAttributeValue>> GetPredefinedFormAttributeValuesAsync(int formAttributeId);

    Task InsertFormAttributeAsync(FormAttribute formAttribute);

    Task InsertFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping);

    Task InsertFormAttributeValueAsync(FormAttributeValue formAttributeValue);

    Task InsertPredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue);

    Task UpdateFormAttributeAsync(FormAttribute formAttribute);

    Task UpdateFormAttributeMappingAsync(FormAttributeMapping formAttributeMapping);

    Task UpdateFormAttributeValueAsync(FormAttributeValue formAttributeValue);

    Task UpdatePredefinedFormAttributeValueAsync(PredefinedFormAttributeValue formAttributeValue);
}