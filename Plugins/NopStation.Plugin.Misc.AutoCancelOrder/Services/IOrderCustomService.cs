using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Misc.AutoCancelOrder.Services
{
    public interface IOrderCustomService
    {
        Task<IPagedList<Order>> SearchOrders(IList<SearchParam> searchParams, int[] oids, int[] sids, int storeId = 0, 
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertOrderNotesAsync(IList<OrderNote> orderNotes);
    }
}