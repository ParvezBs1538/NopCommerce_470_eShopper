using System.Threading.Tasks;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Factories
{
    public interface IOCarouselModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<OCarouselSearchModel> PrepareOCarouselSearchModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselListModel> PrepareOCarouselListModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselModel> PrepareOCarouselModelAsync(OCarouselModel model, OCarousel carousel,
            bool excludeProperties = false);

        Task<OCarouselItemListModel> PrepareOCarouselItemListModelAsync(OCarouselItemSearchModel searchModel, OCarousel carousel);

        Task<AddProductToCarouselSearchModel> PrepareAddProductToOCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel);

        Task<AddProductToCarouselListModel> PrepareAddProductToOCarouselListModelAsync(AddProductToCarouselSearchModel searchModel);
    }
}