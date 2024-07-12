using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public int Order => 1;

    public MapperConfiguration()
    {
        CreateMap<PictureZoomSettings, ConfigurationModel>()
            .ForMember(model => model.AdjustX_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.AdjustY_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.FullSizeImage_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ImageSize_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.LensOpacity_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.LtrPositionTypeId_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.RtlPositionTypeId_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ShowTitle_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.SmoothMove_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.SoftFocus_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.TintOpacity_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.Tint_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.TitleOpacity_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ZoomHeight_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.ZoomWidth_OverrideForStore, options => options.Ignore())
            .ForMember(model => model.CustomProperties, options => options.Ignore())
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, PictureZoomSettings>();
    }
}
