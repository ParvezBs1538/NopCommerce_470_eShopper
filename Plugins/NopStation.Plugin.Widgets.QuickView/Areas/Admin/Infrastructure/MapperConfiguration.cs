using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.QuickView.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.QuickView.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<QuickViewSettings, ConfigurationModel>()
            .ForMember(model => model.ShowRelatedProducts_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowAlsoPurchasedProducts_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowAddToWishlistButton_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowAvailability_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowCompareProductsButton_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowDeliveryInfo_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowFullDescription_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowProductEmailAFriendButton_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowProductManufacturers_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowProductReviewOverview_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowProductSpecifications_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowProductTags_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowRelatedProducts_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowShortDescription_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.EnablePictureZoom_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.CustomProperties, options => options.Ignore())
            .ForMember(model => model.PictureZoomPluginInstalled, options => options.Ignore())
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, QuickViewSettings>();
    }
}
