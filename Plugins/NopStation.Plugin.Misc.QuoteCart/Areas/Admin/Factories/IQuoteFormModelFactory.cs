using System.Threading.Tasks;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public partial interface IQuoteFormModelFactory
{
    public Task<QuoteFormSearchModel> PrepareFormSearchModelAsync(QuoteFormSearchModel searchModel);

    public Task<QuoteFormListModel> PrepareFormListModelAsync(QuoteFormSearchModel searchModel);

    public Task<QuoteFormModel> PrepareQuoteFormModelAsync(QuoteFormModel model, QuoteForm form, bool excludeProperties = false);

    public Task UpdateLocalesAsync(QuoteForm form, QuoteFormModel model);

    public Task SaveStoreMappingsAsync(QuoteForm form, QuoteFormModel model);

    Task<FormAttributeMappingListModel> PrepareFormAttributeMappingListModelAsync(FormAttributeMappingSearchModel searchModel, QuoteForm form);

    Task<FormAttributeMappingModel> PrepareFormAttributeMappingModelAsync(FormAttributeMappingModel model, QuoteForm quoteForm, FormAttributeMapping formAttributeMapping, bool excludeProperties = false);

    Task<FormAttributeValueListModel> PrepareFormAttributeValueListModelAsync(FormAttributeValueSearchModel searchModel, FormAttributeMapping formAttributeMapping);

    Task<FormAttributeValueModel> PrepareFormAttributeValueModelAsync(FormAttributeValueModel model, FormAttributeMapping formAttributeMapping, FormAttributeValue formAttributeValue, bool excludeProperties = false);
}
