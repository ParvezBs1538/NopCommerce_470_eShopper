using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;

namespace NopStation.Plugin.Misc.MergeGuestOrder.Services
{
    public interface IOrderServiceCustom
    {
        Task<IPagedList<Order>> SearchOrders(string email, CheckEmailInAddress checkIn,
                int pageIndex = 0, int pageSize = int.MaxValue);

        Task UpdateOrdersAsync(IList<Order> orders);

        Task InsertOrderNotesAsync(IList<OrderNote> orderNotes);
    }
}