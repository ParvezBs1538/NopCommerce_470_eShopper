using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.NopChat.Areas.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<NopChatSettings, ConfigurationModel>();
            CreateMap<ConfigurationModel, NopChatSettings>();
        }

        public int Order => 0;
    }
}
