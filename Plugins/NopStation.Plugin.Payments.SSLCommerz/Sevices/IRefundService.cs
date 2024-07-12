using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.SSLCommerz.Domains;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices
{
    public interface IRefundService
    {
        Task DeleteRefundAsync(Refund refund);

        Task InsertRefundAsync(Refund refund);

        Task UpdateRefundAsync(Refund refund);

        Task<Refund> GetRefundByIdAsync(int refundId);

        Task<IPagedList<Refund>> SearchRefundsAsync(int orderId = 0, bool? refunded = null, int maxCheckCount = 0,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}