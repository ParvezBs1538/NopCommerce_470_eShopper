using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class OrderDeliverySlotService : IOrderDeliverySlotService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDeliverySlot> _orderDeliverySlotRepository;

        #endregion

        #region Ctor

        public OrderDeliverySlotService(IRepository<Order> orderRepository,
            IRepository<OrderDeliverySlot> orderDeliverySlotRepository)
        {
            _orderRepository = orderRepository;
            _orderDeliverySlotRepository = orderDeliverySlotRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteOrderDeliverySlotAsync(OrderDeliverySlot orderDeliverySlot)
        {
            await _orderDeliverySlotRepository.DeleteAsync(orderDeliverySlot);
        }

        public async Task InsertOrderDeliverySlot(OrderDeliverySlot orderDeliverySlot)
        {
            await _orderDeliverySlotRepository.InsertAsync(orderDeliverySlot);
        }

        public async Task UpdateOrderDeliverySlot(OrderDeliverySlot orderDeliverySlot)
        {
            await _orderDeliverySlotRepository.UpdateAsync(orderDeliverySlot);
        }

        public async Task<OrderDeliverySlot> GetOrderDeliverySlotByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;

            return await _orderDeliverySlotRepository.Table.FirstOrDefaultAsync(x => x.OrderId == orderId);
        }

        public async Task<IList<OrderDeliverySlot>> GetOrderDeliverySlotsByDateAndSlotId(DateTime date, int deliverySlotId, int shippingMethodId, int storeId)
        {
            if (deliverySlotId == 0)
                return new List<OrderDeliverySlot>();

            var query = from ods in _orderDeliverySlotRepository.Table
                        join o in _orderRepository.Table on ods.OrderId equals o.Id
                        where !o.Deleted && ods.DeliverySlotId == deliverySlotId &&
                        o.StoreId == storeId &&
                        ods.DeliveryDate.Date == date.Date &&
                        ods.ShippingMethodId == shippingMethodId
                        select ods;

            return await query.ToListAsync();
        }

        public async Task<IPagedList<OrderDeliverySlot>> SearchOrderDeliverySlots(DateTime? fromDate = null,
            DateTime? toDate = null, int shippingMethodId = 0, int deliverySlotId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (fromDate.HasValue)
                fromDate = fromDate.Value.Date;
            if (toDate.HasValue)
                toDate = toDate.Value.Date;

            var query = from ods in _orderDeliverySlotRepository.Table
                        join o in _orderRepository.Table on ods.OrderId equals o.Id
                        where !o.Deleted && 
                        (deliverySlotId == 0 || ods.DeliverySlotId == deliverySlotId) &&
                        (fromDate == null || ods.DeliveryDate.Date >= fromDate) &&
                        (toDate == null || ods.DeliveryDate.Date <= toDate) &&
                        (shippingMethodId == 0 || ods.ShippingMethodId == shippingMethodId)
                        select ods;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }


        #endregion
    }
}
