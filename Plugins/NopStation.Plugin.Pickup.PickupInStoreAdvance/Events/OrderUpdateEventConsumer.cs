using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Events;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Events
{
    public class OrderUpdateEventConsumer : IConsumer<EntityUpdatedEvent<Order>>
    {
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;

        public OrderUpdateEventConsumer(IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService)
        {
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
        }
        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            if (!order.PickupInStore)
            {
                return;
            }
            if (order.OrderStatus == OrderStatus.Complete)
            {
                var pickupInStoreDeliveryManage = await _pickupInStoreDeliveryManageService.GetPickupInStoreDeliverManageByOrderIdAsync(order.Id);
                if (pickupInStoreDeliveryManage != null && pickupInStoreDeliveryManage.PickUpStatusType != PickUpStatusType.PickedUpByCustomer)
                {
                    pickupInStoreDeliveryManage.PickUpStatusType = PickUpStatusType.PickedUpByCustomer;
                    pickupInStoreDeliveryManage.CustomerPickedUpAtUtc = DateTime.UtcNow;
                    await _pickupInStoreDeliveryManageService.UpdateAsync(pickupInStoreDeliveryManage);
                }
            }
            else if (order.OrderStatus == OrderStatus.Cancelled)
            {
                var pickupInStoreDeliveryManage = await _pickupInStoreDeliveryManageService.GetPickupInStoreDeliverManageByOrderIdAsync(order.Id);
                if (pickupInStoreDeliveryManage != null && pickupInStoreDeliveryManage.PickUpStatusType != PickUpStatusType.OrderCanceled)
                {
                    pickupInStoreDeliveryManage.PickUpStatusType = PickUpStatusType.OrderCanceled;
                    await _pickupInStoreDeliveryManageService.UpdateAsync(pickupInStoreDeliveryManage);
                }
            }
        }
    }
}
