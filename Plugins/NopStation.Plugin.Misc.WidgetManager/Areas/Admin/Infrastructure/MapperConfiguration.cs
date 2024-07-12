using AutoMapper;
using AutoMapper.Internal;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;
using NopStation.Plugin.Misc.WidgetManager.Domain.Widgets;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<WidgetZoneMapping, WidgetZoneModel>();
        CreateMap<WidgetZoneModel, WidgetZoneMapping>();

        CreateMap<CustomerConditionMapping, CustomerConditionModel>();
        CreateMap<CustomerConditionModel, CustomerConditionMapping>();

        CreateMap<ProductConditionMapping, ProductConditionModel>();
        CreateMap<ProductConditionModel, ProductConditionMapping>();

        this.Internal().ForAllMaps((mapConfiguration, map) =>
          {
              if (typeof(IScheduleSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
              {
                  map.ForMember(nameof(IScheduleSupportedModel.Schedule), options => options.Ignore());
              }
          });
    }

    public int Order => 1;
}
