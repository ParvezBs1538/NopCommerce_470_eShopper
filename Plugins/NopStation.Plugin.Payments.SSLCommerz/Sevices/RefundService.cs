using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Payments.SSLCommerz.Domains;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices
{
    public class RefundService : IRefundService
    {
        #region Fields

        private readonly IRepository<Refund> _refundRepository;

        #endregion

        #region Ctor

        public RefundService(IRepository<Refund> refundRepository)
        {
            _refundRepository = refundRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteRefundAsync(Refund refund)
        {
            await _refundRepository.DeleteAsync(refund);
        }

        public async Task InsertRefundAsync(Refund refund)
        {
            await _refundRepository.InsertAsync(refund);
        }

        public async Task UpdateRefundAsync(Refund refund)
        {
            await _refundRepository.UpdateAsync(refund);
        }

        public async Task<Refund> GetRefundByIdAsync(int refundId)
        {
            if (refundId == 0)
                return null;

            return await _refundRepository.GetByIdAsync(refundId);
        }

        public async Task<IPagedList<Refund>> SearchRefundsAsync(int orderId = 0, bool? refunded = null, int maxCheckCount = 0,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from j in _refundRepository.Table
                        where (orderId == 0 || j.OrderId == orderId) &&
                            (refunded == null || j.Refunded == refunded.Value) &&
                            j.StatusCheckCount <= maxCheckCount
                        orderby j.CreatedOnUtc descending
                        select j;

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
