using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 100;
        
        public MapperConfiguration()
        {
            CreateMap<PickupInStoreDeliveryManage, PickupInStoreDeliveryManageModel>();
            CreateMap<PickupInStoreDeliveryManageModel, PickupInStoreDeliveryManage>();
        }
    }
}
