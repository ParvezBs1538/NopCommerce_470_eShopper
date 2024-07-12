using System.Threading.Tasks;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public interface IFormAttributeModelFactory
{
    Task<FormAttributeFormListModel> PrepareFormAttributeFormListModelAsync(FormAttributeFormSearchModel searchModel, FormAttribute formAttribute);

    Task<FormAttributeListModel> PrepareFormAttributeListModelAsync(FormAttributeSearchModel searchModel);

    Task<FormAttributeModel> PrepareFormAttributeModelAsync(FormAttributeModel model, FormAttribute formAttribute, bool excludeProperties = false);

    Task<FormAttributeSearchModel> PrepareFormAttributeSearchModelAsync(FormAttributeSearchModel searchModel);

    Task<PredefinedFormAttributeValueListModel> PreparePredefinedFormAttributeValueListModelAsync(PredefinedFormAttributeValueSearchModel searchModel, FormAttribute formAttribute);

    Task<PredefinedFormAttributeValueModel> PreparePredefinedFormAttributeValueModelAsync(PredefinedFormAttributeValueModel model, FormAttribute formAttribute, PredefinedFormAttributeValue predefinedFormAttributeValue, bool excludeProperties = false);
}