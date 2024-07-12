using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<SmartDealCarouselSettings, ConfigurationModel>()
                .ForMember(model => model.EnableCarousel_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, SmartDealCarouselSettings>();

        CreateMap<SmartDealCarousel, SmartDealCarouselModel>()
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.AvailableProductSources, options => options.Ignore())
                .ForMember(model => model.ProductSourceTypeStr, options => options.Ignore())
                .ForMember(model => model.BackgroundTypeStr, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.CarouselProductSearchModel, options => options.Ignore())
                .ForMember(model => model.CarouselProductSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
        CreateMap<SmartDealCarouselModel, SmartDealCarousel>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

        CreateMap<SmartDealCarouselProductMapping, SmartDealCarouselProductModel>()
                .ForMember(model => model.ProductName, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.PictureUrl, options => options.Ignore());
        CreateMap<SmartDealCarouselProductModel, SmartDealCarouselProductMapping>();
    }
}
