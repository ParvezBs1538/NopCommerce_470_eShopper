using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;
using NopStation.Plugin.Shipping.VendorAndState.Domain;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<VendorAndStateSettings, ConfigurationModel>()
                .ForMember(model => model.EnablePlugin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
            CreateMap<ConfigurationModel, VendorAndStateSettings>();

            CreateMap<VendorShipping, VendorShippingModel>()
                .ForMember(model => model.AvailableShippingMethods, options => options.Ignore())
                .ForMember(model => model.ShippingMethod, options => options.Ignore())
                .ForMember(model => model.ShippingMethod, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore())
                .ForMember(model => model.VendorName, options => options.Ignore());
            CreateMap<VendorShippingModel, VendorShipping>();

            CreateMap<VendorStateProvinceShipping, VendorStateProvinceShippingModel>()
                .ForMember(model => model.StateProvince, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());
            CreateMap<VendorStateProvinceShippingModel, VendorStateProvinceShipping>();
        }
    }
}
