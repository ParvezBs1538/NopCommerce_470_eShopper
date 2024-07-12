using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<VendorShopSettings, ConfigurationModel>()
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, VendorShopSettings>();

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

            CreateMap<Slider, SliderModel>()
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.AvailableWidgetZones, options => options.Ignore())
                    .ForMember(model => model.WidgetZoneStr, options => options.Ignore())
                    .ForMember(model => model.CreatedOn, options => options.Ignore())
                    .ForMember(model => model.UpdatedOn, options => options.Ignore())
                    .ForMember(model => model.SliderItemSearchModel, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<SliderModel, Slider>()
                    .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                    .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<SliderItem, SliderItemModel>()
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.FullPictureUrl, options => options.Ignore())
                    .ForMember(model => model.PictureUrl, options => options.Ignore())
                    .ForMember(model => model.MobileFullPictureUrl, options => options.Ignore())
                    .ForMember(model => model.MobilePictureUrl, options => options.Ignore())
                    .ForMember(model => model.SliderItemTitle, options => options.MapFrom(s => s.Title));
            CreateMap<SliderItemModel, SliderItem>()
                    .ForMember(model => model.Title, options => options.MapFrom(s => s.SliderItemTitle));

            // vendor profile mapper
            CreateMap<VendorProfile, VendorProfileModel>();
            CreateMap<VendorProfileModel, VendorProfile>();

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
