using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<RedxSettings, ConfigurationModel>()
                .ForMember(model => model.ApiAccessToken_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BaseUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ParcelTrackUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.UseSandbox_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SandboxUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShipmentEventsUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, RedxSettings>();

            CreateMap<RedxArea, RedxAreaModel>()
                .ForMember(model => model.AvailableStateProvinces, options => options.Ignore());
            CreateMap<RedxAreaModel, RedxArea>();

            CreateMap<RedxShipment, RedxShipmentModel>()
                .ForMember(model => model.AvailableRedxAreas, options => options.Ignore())
                .ForMember(model => model.LabelUrl, options => options.Ignore())
                .ForMember(model => model.CustomOrderNumber, options => options.Ignore())
                .ForMember(model => model.OrderStatusId, options => options.Ignore())
                .ForMember(model => model.OrderStatus, options => options.Ignore());
            CreateMap<RedxShipmentModel, RedxShipment>();
        }
    }
}