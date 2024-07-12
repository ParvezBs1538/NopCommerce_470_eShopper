using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<OrderRatingSettings, ConfigurationModel>()
                .ForMember(model => model.ShowOrderRatedDateInDetailsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.OrderDetailsPageWidgetZone_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.OpenOrderRatingPopupInHomepage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, OrderRatingSettings>();
        }
    }
}
