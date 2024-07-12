using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface IOrderDeliverySlotService
    {
        Task DeleteOrderDeliverySlotAsync(OrderDeliverySlot orderDeliverySlot);

        Task InsertOrderDeliverySlot(OrderDeliverySlot orderDeliverySlot);

        Task UpdateOrderDeliverySlot(OrderDeliverySlot orderDeliverySlot);

        Task<OrderDeliverySlot> GetOrderDeliverySlotByOrderId(int orderId);

        Task<IList<OrderDeliverySlot>> GetOrderDeliverySlotsByDateAndSlotId(DateTime date, int deliverySlotId, int shippingMethodId, int storeId);

        Task<IPagedList<OrderDeliverySlot>> SearchOrderDeliverySlots(DateTime? fromDate = null, DateTime? toDate = null, 
            int shippingMethodId = 0, int deliverySlotId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    }
}