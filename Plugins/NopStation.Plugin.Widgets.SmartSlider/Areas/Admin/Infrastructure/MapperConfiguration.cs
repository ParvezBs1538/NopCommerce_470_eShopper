using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<SmartSliderSettings, ConfigurationModel>()
                .ForMember(model => model.EnableAjaxLoad_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableSlider_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SupportedVideoExtensions, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, SmartSliderSettings>()
                .ForMember(setting => setting.SupportedVideoExtensions, options => options.Ignore());

        CreateMap<SmartSlider, SmartSliderModel>()
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.UpdatedOn, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.SelectedStoreIds, options => options.Ignore());

        CreateMap<SmartSliderModel, SmartSlider>()
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

        CreateMap<SmartSliderItem, SmartSliderItemModel>()
                .ForMember(model => model.CustomProperties, options => options.Ignore());

        CreateMap<SmartSliderItemModel, SmartSliderItem>();



    }
}
