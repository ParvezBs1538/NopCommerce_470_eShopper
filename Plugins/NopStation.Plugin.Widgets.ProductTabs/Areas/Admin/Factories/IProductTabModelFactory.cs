using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Factories
{
    public interface IProductTabModelFactory
    {
        Task<ProductTabSearchModel> PrepareOCarouselSearchModelAsync(ProductTabSearchModel searchModel);

        Task<ProductTabListModel> PrepareProductTabListModelAsync(ProductTabSearchModel searchModel);

        Task<ProductTabModel> PrepareProductTabModelAsync(ProductTabModel model, ProductTab productTab,
            bool excludeProperties = false);

        Task<ProductTabItemListModel> PrepareProductTabItemListModelAsync(ProductTabItemSearchModel searchModel, ProductTab productTab);

        Task<ProductTabItemModel> PrepareProductTabItemModelAsync(ProductTabItemModel model, ProductTabItem productTabItem,
            ProductTab productTab, bool excludeProperties = false);

        Task<ProductTabItemProductListModel> PrepareProductTabItemProductListModelAsync(ProductTabItemProductSearchModel searchModel,
            ProductTabItem productTabItem);

        Task<AddProductToProductTabItemSearchModel> PrepareAddProductToProductTabItemSearchModelAsync(AddProductToProductTabItemSearchModel searchModel);

        Task<AddProductToProductTabItemListModel> PrepareAddProductToProductTabItemListModelAsync(AddProductToProductTabItemSearchModel searchModel);
    }
}