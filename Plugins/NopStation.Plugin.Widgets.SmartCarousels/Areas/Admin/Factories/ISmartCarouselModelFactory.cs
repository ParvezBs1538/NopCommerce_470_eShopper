using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Factories;

public interface ISmartCarouselModelFactory
{
    #region Configuration

    Task<ConfigurationModel> PrepareConfigurationModelAsync();

    #endregion

    #region Carousels

    Task<SmartCarouselSearchModel> PrepareCarouselSearchModelAsync(SmartCarouselSearchModel searchModel);

    Task<SmartCarouselListModel> PrepareCarouselListModelAsync(SmartCarouselSearchModel searchModel);

    Task<SmartCarouselModel> PrepareCarouselModelAsync(SmartCarouselModel model, SmartCarousel carousel, bool excludeProperties = false);

    #endregion

    #region Carousel products

    Task<SmartCarouselProductListModel> PrepareCarouselProductListModelAsync(SmartCarouselProductSearchModel searchModel, SmartCarousel carousel);

    Task<AddProductToCarouselSearchModel> PrepareAddProductToCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel);

    Task<AddProductToCarouselListModel> PrepareAddProductToCarouselListModelAsync(AddProductToCarouselSearchModel searchModel);

    #endregion

    #region Carousel manufacturers

    Task<SmartCarouselManufacturerListModel> PrepareCarouselManufacturerListModelAsync(SmartCarouselManufacturerSearchModel searchModel, SmartCarousel carousel);

    Task<AddManufacturerToCarouselSearchModel> PrepareAddManufacturerToCarouselSearchModelAsync(AddManufacturerToCarouselSearchModel searchModel);

    Task<AddManufacturerToCarouselListModel> PrepareAddManufacturerToCarouselListModelAsync(AddManufacturerToCarouselSearchModel searchModel);

    #endregion

    #region Carousel categories

    Task<SmartCarouselCategoryListModel> PrepareCarouselCategoryListModelAsync(SmartCarouselCategorySearchModel searchModel, SmartCarousel carousel);

    Task<AddCategoryToCarouselSearchModel> PrepareAddCategoryToCarouselSearchModelAsync(AddCategoryToCarouselSearchModel searchModel);

    Task<AddCategoryToCarouselListModel> PrepareAddCategoryToCarouselListModelAsync(AddCategoryToCarouselSearchModel searchModel);

    #endregion

    #region Carousel vendor

    Task<SmartCarouselVendorListModel> PrepareCarouselVendorListModelAsync(SmartCarouselVendorSearchModel searchModel, SmartCarousel carousel);

    Task<AddVendorToCarouselSearchModel> PrepareAddVendorToCarouselSearchModelAsync(AddVendorToCarouselSearchModel searchModel);

    Task<AddVendorToCarouselListModel> PrepareAddVendorToCarouselListModelAsync(AddVendorToCarouselSearchModel searchModel);

    #endregion

    #region Carousel pictures

    Task<SmartCarouselPictureListModel> PrepareCarouselPictureListModelAsync(SmartCarouselPictureSearchModel searchModel, SmartCarousel carousel);

    Task<SmartCarouselPictureModel> PrepareCarouselPictureModelAsync(SmartCarouselPictureModel model,
        SmartCarouselPictureMapping pictureMapping, SmartCarousel carousel, bool excludeProperties = false);

    #endregion
}