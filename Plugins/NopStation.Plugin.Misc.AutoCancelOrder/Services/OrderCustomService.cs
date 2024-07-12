using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;

namespace NopStation.Plugin.Misc.AutoCancelOrder.Services
{
    public class OrderCustomService : IOrderCustomService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;

        public OrderCustomService(IRepository<Order> orderRepository,
            IRepository<OrderNote> orderNoteRepository)
        {
            _orderRepository = orderRepository;
            _orderNoteRepository = orderNoteRepository;
        }

        public async Task InsertOrderNotesAsync(IList<OrderNote> orderNotes)
        {
            await _orderNoteRepository.InsertAsync(orderNotes);
        }

        public async Task<IPagedList<Order>> SearchOrders(IList<SearchParam> searchParams, int[] oids, int[] sids, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (oids == null || sids == null || searchParams == null)
                return new PagedList<Order>(new List<Order>(), pageIndex, pageSize);

            var query = from o in _orderRepository.Table
                        where oids.Contains(o.OrderStatusId) &&
                        sids.Contains(o.ShippingStatusId) &&
                        (storeId == 0 || o.StoreId == storeId) &&
                        o.PaymentStatusId == (int)PaymentStatus.Pending
                        select o;

            var query1 = from o in _orderRepository.Table
                         where o.Id == 0
                         select o;
            foreach (var param in searchParams)
            {
                var query3 = from o in query
                             where o.CreatedOnUtc <= param.CreatedOnUtc && 
                             o.PaymentMethodSystemName == param.PaymentMethodSystemName
                             select o;

                query1 = query1.Concat(query3);
            }

            return await query1.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
