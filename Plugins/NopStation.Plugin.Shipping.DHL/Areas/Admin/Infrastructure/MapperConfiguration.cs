using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Shipping.DHL.Areas.Admin.Models;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Settings

            CreateMap<DHLSettings, ConfigurationModel>()
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore())
                .ForMember(model => model.AvailableCurrencies, options => options.Ignore());
            CreateMap<ConfigurationModel, DHLSettings>();

            #endregion
        }

        public int Order => 0;
    }
}
