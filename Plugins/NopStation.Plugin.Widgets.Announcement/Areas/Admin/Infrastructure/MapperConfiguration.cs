using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<AnnouncementSettings, ConfigurationModel>()
            .ForMember(model => model.EnablePlugin_OverrideForStore, option => option.Ignore())
            .ForMember(model => model.ItemSeparator_OverrideForStore, option => option.Ignore())
            .ForMember(model => model.WidgetZone_OverrideForStore, option => option.Ignore());
        CreateMap<ConfigurationModel, AnnouncementSettings>();

        CreateMap<AnnouncementItem, AnnouncementItemModel>();
        CreateMap<AnnouncementItemModel, AnnouncementItem>();
    }

    public int Order => 1;
}
