using Nop.Core;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public interface IDHLOrderService
    {
        Task<IPagedList<Order>> SearchOrderAsync(int pageIndex = 0, int pageSize = int.MaxValue, int orderId = 0);
    }
}
