using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Factories;

public interface IConditionModelFactory
{
    #region Customer condition mappings

    Task PrepareCustomerConditionMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported
        where TModel : ICustomerConditionSupportedModel;

    Task<CustomerConditionListModel> PrepareCustomerConditionMappingListModelAsync(CustomerConditionSearchModel searchModel);

    Task<AddCustomerToConditionSearchModel> PrepareAddCustomerToConditionSearchModelAsync<TEntity>(AddCustomerToConditionSearchModel searchModel, TEntity entity)
        where TEntity : BaseEntity, ICustomerConditionSupported;

    Task<AddCustomerToConditionListModel> PrepareAddCustomerToConditionListModelAsync(AddCustomerToConditionSearchModel searchModel);

    #endregion

    #region Product condition mappings

    Task PrepareProductConditionMappingSearchModelAsync<TModel, TEntity>(TModel model, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported
        where TModel : IProductConditionSupportedModel;

    Task<ProductConditionListModel> PrepareProductConditionMappingListModelAsync(ProductConditionSearchModel searchModel);

    Task<AddProductToConditionSearchModel> PrepareAddProductToConditionSearchModelAsync<TEntity>(AddProductToConditionSearchModel searchModel, TEntity entity)
        where TEntity : BaseEntity, IProductConditionSupported;

    Task<AddProductToConditionListModel> PrepareAddProductToConditionListModelAsync(AddProductToConditionSearchModel searchModel);

    #endregion
}