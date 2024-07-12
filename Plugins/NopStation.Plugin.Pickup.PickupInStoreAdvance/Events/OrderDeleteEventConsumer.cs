using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Events;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Events
{
    public class OrderDeleteEventConsumer : IConsumer<EntityDeletedEvent<Order>>
    {
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;

        public OrderDeleteEventConsumer(IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService)
        {
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
        }
        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            var pickupInStoreDeliveryManage = await _pickupInStoreDeliveryManageService.GetPickupInStoreDeliverManageByOrderIdAsync(eventMessage.Entity.Id);
            if (pickupInStoreDeliveryManage != null)
            {
                await _pickupInStoreDeliveryManageService.DeleteAsync(pickupInStoreDeliveryManage);
            }
        }
    }
}
