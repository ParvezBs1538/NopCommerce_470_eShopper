using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<ProductTabSettings, ConfigurationModel>()
                    .ForMember(model => model.EnableProductTab_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, ProductTabSettings>();

            CreateMap<ProductTab, ProductTabModel>()
                    .ForMember(model => model.WidgetZoneStr, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ProductTabItemSearchModel, options => options.Ignore())
                    .ForMember(model => model.AvailableWidgetZones, options => options.Ignore())
                    .ForMember(model => model.Locales, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore())
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore());
            CreateMap<ProductTabModel, ProductTab>()
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.LimitedToStores, options => options.Ignore());

            CreateMap<ProductTabItem, ProductTabItemModel>()
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ProductSearchModel, options => options.Ignore())
                    .ForMember(model => model.Locales, options => options.Ignore());
            CreateMap<ProductTabItemModel, ProductTabItem>();

            CreateMap<ProductTabItemProduct, ProductTabItemProductModel>()
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ProductName, options => options.Ignore());
            CreateMap<ProductTabItemProductModel, ProductTabItemProduct>();
        }
    }
}
