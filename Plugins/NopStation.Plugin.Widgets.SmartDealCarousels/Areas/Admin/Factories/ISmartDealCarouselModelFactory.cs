using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Factories;

public interface ISmartDealCarouselModelFactory
{
    #region Configuration

    Task<ConfigurationModel> PrepareConfigurationModelAsync();

    #endregion

    #region Carousels

    Task<SmartDealCarouselSearchModel> PrepareCarouselSearchModelAsync(SmartDealCarouselSearchModel searchModel);

    Task<SmartDealCarouselListModel> PrepareCarouselListModelAsync(SmartDealCarouselSearchModel searchModel);

    Task<SmartDealCarouselModel> PrepareCarouselModelAsync(SmartDealCarouselModel model, SmartDealCarousel carousel, bool excludeProperties = false);

    #endregion

    #region Carousel products

    Task<SmartDealCarouselProductListModel> PrepareCarouselProductListModelAsync(SmartDealCarouselProductSearchModel searchModel, SmartDealCarousel carousel);

    Task<AddProductToCarouselSearchModel> PrepareAddProductToCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel);

    Task<AddProductToCarouselListModel> PrepareAddProductToCarouselListModelAsync(AddProductToCarouselSearchModel searchModel);

    #endregion
}