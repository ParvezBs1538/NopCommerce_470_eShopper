using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<SmartCarouselSettings, ConfigurationModel>()
                .ForMember(model => model.EnableCarousel_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, SmartCarouselSettings>();

        CreateMap<SmartCarousel, SmartCarouselModel>()
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.AvailableProductSources, options => options.Ignore())
                .ForMember(model => model.ProductSourceTypeStr, options => options.Ignore())
                .ForMember(model => model.BackgroundTypeStr, options => options.Ignore())
                .ForMember(model => model.CarouselTypeStr, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.CarouselProductSearchModel, options => options.Ignore())
                .ForMember(model => model.CarouselProductSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
        CreateMap<SmartCarouselModel, SmartCarousel>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

        CreateMap<SmartCarouselProductMapping, SmartCarouselProductModel>()
                .ForMember(model => model.ProductName, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.PictureUrl, options => options.Ignore());
        CreateMap<SmartCarouselProductModel, SmartCarouselProductMapping>();

        CreateMap<SmartCarouselManufacturerMapping, SmartCarouselManufacturerModel>()
                .ForMember(model => model.ManufacturerName, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
        CreateMap<SmartCarouselManufacturerModel, SmartCarouselManufacturerMapping>();

        CreateMap<SmartCarouselCategoryMapping, SmartCarouselCategoryModel>()
                .ForMember(model => model.CategoryName, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
        CreateMap<SmartCarouselCategoryModel, SmartCarouselCategoryMapping>();

        CreateMap<SmartCarouselVendorMapping, SmartCarouselVendorModel>()
                .ForMember(model => model.VendorName, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
        CreateMap<SmartCarouselVendorModel, SmartCarouselVendorMapping>();

        CreateMap<SmartCarouselPictureMapping, SmartCarouselPictureModel>()
                .ForMember(model => model.OverrideAltAttribute, options => options.Ignore())
                .ForMember(model => model.OverrideTitleAttribute, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.PictureUrl, options => options.Ignore());
        CreateMap<SmartCarouselPictureModel, SmartCarouselPictureMapping>();
    }
}
