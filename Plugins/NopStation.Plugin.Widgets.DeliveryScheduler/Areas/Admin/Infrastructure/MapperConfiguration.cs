using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;

        public MapperConfiguration()
        {
            CreateMap<DeliverySlotModel, DeliverySlot>()
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());
            CreateMap<DeliverySlot, DeliverySlotModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());

            CreateMap<DeliveryCapacityModel, DeliveryCapacity>();
            CreateMap<DeliveryCapacity, DeliveryCapacityModel>()
                .ForMember(model => model.DeliverySlot, options => options.Ignore());

            CreateMap<SpecialDeliveryCapacityModel, SpecialDeliveryCapacity>();
            CreateMap<SpecialDeliveryCapacity, SpecialDeliveryCapacityModel>()
                .ForMember(model => model.SpecialDateStr, options => options.Ignore())
                .ForMember(model => model.DeliverySlot, options => options.Ignore());

            CreateMap<ConfigurationModel, DeliverySchedulerSettings>();
            CreateMap<DeliverySchedulerSettings, ConfigurationModel>()
                .ForMember(model => model.DisplayDayOffset_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EnableScheduling_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NumberOfDaysToDisplay_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DateFormat_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CategorySearchModel, options => options.Ignore())
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());

            CreateMap<OrderDeliverySlotModel, OrderDeliverySlot>();
            CreateMap<OrderDeliverySlot, OrderDeliverySlotModel>()
                .ForMember(model => model.DeliverySlot, option => option.Ignore())
                .ForMember(model => model.ShippingMethod, option => option.Ignore())
                .ForMember(model => model.AvailableDeliverySlots, option => option.Ignore());
        }
    }
}
