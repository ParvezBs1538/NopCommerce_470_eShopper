using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Events;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Events
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;

        public OrderPlacedEventConsumer(IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService)
        {
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
        }
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (eventMessage.Order.PickupInStore)
            {
                var pickup = new PickupInStoreDeliveryManage
                {
                    OrderId = eventMessage.Order.Id,
                    PickUpStatusTypeId = (int)PickUpStatusType.OrderInitied,
                    ReadyForPickupMarkedAtUtc = null,
                    CreatedShipmentId = null,
                    CustomerPickedUpAtUtc = null
                };
                await _pickupInStoreDeliveryManageService.InsertAsync(pickup);
            }

        }
    }
}
