using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class EventReportService : IEventReportService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;

        public EventReportService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IList<int>> GetCustomerMinInteractions()
        {
            var query = from order in _orderRepository.Table
                        group order by order.CustomerId into grp
                        where grp.Count() > 1
                        select grp.Key;

            return await query.ToListAsync();
        }

        public async Task<IPagedList<EventReportLine>> GetEventReportLineAsync(int storeId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            if (pageSize == int.MaxValue)
                pageSize = pageSize - 1;

            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var query = from orderItem in _orderItemRepository.Table
                        join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                        !o.Deleted &&
                        (!fromDate.HasValue || fromDate.Value <= o.CreatedOnUtc) &&
                        (!toDate.HasValue || toDate.Value >= o.CreatedOnUtc)

                        select new EventReportLine
                        {
                            UserId = o.CustomerId,
                            ItemId = orderItem.ProductId,
                            CreatedOnUtc = o.CreatedOnUtc
                        };
           

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}