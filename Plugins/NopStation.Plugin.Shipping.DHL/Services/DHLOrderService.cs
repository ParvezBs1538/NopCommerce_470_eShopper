using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public class DHLOrderService : IDHLOrderService
    {
        private readonly IRepository<Order> _orderRepository;

        public DHLOrderService(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<IPagedList<Order>> SearchOrderAsync(int pageIndex = 0, int pageSize = int.MaxValue, int orderId = 0)
        {
            var orders = from o in _orderRepository.Table
                         where o.ShippingRateComputationMethodSystemName.Contains("NopStation.Plugin.Shipping.DHL") && o.Deleted == false
                         orderby o.CreatedOnUtc descending
                         select o;

            if (orderId != 0)
            {
                orders = from o in orders
                         where o.Id == orderId
                         orderby o.CreatedOnUtc descending
                         select o;
            }

            return orders.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
