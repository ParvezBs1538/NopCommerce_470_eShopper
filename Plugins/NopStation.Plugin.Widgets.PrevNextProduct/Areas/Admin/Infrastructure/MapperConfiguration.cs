using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<PrevNextProductSettings, ConfigurationModel>();
        CreateMap<ConfigurationModel, PrevNextProductSettings>();
    }

    public int Order => 1;
}
