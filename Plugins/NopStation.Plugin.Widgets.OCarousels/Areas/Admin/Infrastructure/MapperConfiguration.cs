using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Models;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<OCarouselSettings, ConfigurationModel>()
                    .ForMember(model => model.EnableOCarousel_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, OCarouselSettings>();

            CreateMap<OCarousel, OCarouselModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.AvailableWidgetZones, options => options.Ignore())
                    .ForMember(model => model.AvailableDataSources, options => options.Ignore())
                    .ForMember(model => model.DataSourceTypeStr, options => options.Ignore())
                    .ForMember(model => model.WidgetZoneStr, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore())
                    .ForMember(model => model.OCarouselItemSearchModel, options => options.Ignore())
                    .ForMember(model => model.OCarouselItemSearchModel, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<OCarouselModel, OCarousel>()
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<OCarouselItem, OCarouselItemModel>()
                    .ForMember(model => model.ProductName, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.PictureUrl, options => options.Ignore());
            CreateMap<OCarouselItemModel, OCarouselItem>();
        }
    }
}
