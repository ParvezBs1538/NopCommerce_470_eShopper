using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public MapperConfiguration()
        {
            CreateMap<AbandonedCart, AbandonedCartModel>().ReverseMap();
            CreateMap<CustomerAbandonmentInfo, CustomerAbandonmentInfoModel>().ReverseMap();
        }

        public int Order => 1;
    }
}