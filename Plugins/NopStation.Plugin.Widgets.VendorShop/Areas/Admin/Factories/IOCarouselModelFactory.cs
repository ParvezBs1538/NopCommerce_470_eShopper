using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public interface IOCarouselModelFactory
    {
        Task<OCarouselSearchModel> PrepareOCarouselSearchModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselListModel> PrepareOCarouselListModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselModel> PrepareOCarouselModelAsync(OCarouselModel model, OCarousel carousel,
            bool excludeProperties = false);

        Task<OCarouselItemListModel> PrepareOCarouselItemListModelAsync(OCarouselItemSearchModel searchModel, OCarousel carousel);

        Task<AddProductToCarouselSearchModel> PrepareAddProductToOCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel);

        Task<AddProductToCarouselListModel> PrepareAddProductToOCarouselListModelAsync(AddProductToCarouselSearchModel searchModel);
    }
}