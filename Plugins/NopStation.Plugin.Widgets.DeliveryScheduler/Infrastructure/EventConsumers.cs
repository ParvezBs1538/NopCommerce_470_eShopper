using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Infrastructure
{
    public class EventConsumers : IConsumer<OrderPlacedEvent>
    {
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShippingService _shippingService;
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;

        public EventConsumers(ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IShippingService shippingService,
            IDeliverySlotService deliverySlotService,
            IOrderDeliverySlotService orderDeliverySlotService)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _shippingService = shippingService;
            _deliverySlotService = deliverySlotService;
            _orderDeliverySlotService = orderDeliverySlotService;
        }

        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Order.CustomerId);
            var savedSlotInfo = await _genericAttributeService.GetAttributeAsync<string>(customer,
                DeliverySchedulerDefaults.DeliverySlotInfo, eventMessage.Order.StoreId);

            if(string.IsNullOrWhiteSpace(savedSlotInfo))
                return;

            var tokens = savedSlotInfo.Split("___", StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 3)
                return;

            if (DateTime.TryParse(tokens[0], out var deliveryDate) && int.TryParse(tokens[1], out var slotId) 
                && int.TryParse(tokens[2], out var shippingMethodId))
            {
                var slot = await _deliverySlotService.GetDeliverySlotByIdAsync(slotId);
                if (slot == null || slot.Deleted || !slot.Active)
                    return;

                var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(shippingMethodId);
                if (shippingMethod == null)
                    return;

                var orderSlot = new OrderDeliverySlot()
                {
                    DeliveryDate = deliveryDate,
                    DeliverySlotId = slot.Id,
                    OrderId = eventMessage.Order.Id,
                    ShippingMethodId = shippingMethodId
                };
                await _orderDeliverySlotService.InsertOrderDeliverySlot(orderSlot);

                await _genericAttributeService.SaveAttributeAsync(customer,
                    DeliverySchedulerDefaults.DeliverySlotInfo, "", eventMessage.Order.StoreId);
            }
        }
    }
}
