using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Models;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<PaymentOption, PaymentOptionModel>()
                    .ForMember(model => model.Logo, options => options.Ignore())
                    .ForMember(model => model.AvailableStores, options => options.Ignore())
                    .ForMember(model => model.CustomProperties, options => options.Ignore())
                    .ForMember(model => model.SelectedStoreIds, options => options.Ignore());
            CreateMap<PaymentOptionModel, PaymentOption>();
        }
    }
}
