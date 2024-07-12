using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<SliderSettings, ConfigurationModel>()
                    .ForMember(model => model.EnableSlider_OverrideForStore, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, SliderSettings>();

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
        }
    }
}
