using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AllInOneContactUs.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AllInOneContactUs.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 10;

        public MapperConfiguration()
        {
            CreateMap<ARContactUsSettings, WidgetsARContactUsConfigurationModel>();
            CreateMap<WidgetsARContactUsConfigurationModel, ARContactUsSettings>();
        }
    }
}
