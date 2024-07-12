using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Payments.bKash.Models;

namespace NopStation.Plugin.Payments.bKash.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            #region Configuration

            CreateMap<BkashPaymentSettings, ConfigurationModel>();
            CreateMap<ConfigurationModel, BkashPaymentSettings>();

            #endregion 
        }

        public int Order => 0;
    }
}
