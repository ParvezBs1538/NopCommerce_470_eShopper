using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class AbandonedCartTrackingService : IAbandonedCartTrackingService
    {
        #region Fields

        private readonly IRepository<AbandonedCartTracking> _abandonedCartTrackingRepository;

        #endregion

        #region Ctor

        public AbandonedCartTrackingService(IRepository<AbandonedCartTracking> abandonedCartTrackingRepository)
        {
            _abandonedCartTrackingRepository = abandonedCartTrackingRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<AbandonedCartTracking>> GetAllAbandonedCartTrackingsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from u in _abandonedCartTrackingRepository.Table
                        orderby u.Id
                        select u;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }


        public virtual async Task<AbandonedCartTracking> GetAbandonedCartTrackingByCustomerAsync(int customerId)
        {
            var query = from u in _abandonedCartTrackingRepository.Table
                        where u.CustomerId == customerId
                        orderby u.Id
                        select u;

            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task InsertAbandonedCartTrackingAsync(AbandonedCartTracking abandonedCartTracking)
        {
            await _abandonedCartTrackingRepository.InsertAsync(abandonedCartTracking);
        }

        public async Task<AbandonedCartTracking> GetAbandonedCartTrackingByIdAsync(int id)
        {
            var query = _abandonedCartTrackingRepository;
            return await query.GetByIdAsync(id);
        }

        public async Task UpdateAbandonedCartTrackingAsync(AbandonedCartTracking abandonedCartTracking)
        {
            await _abandonedCartTrackingRepository.UpdateAsync(abandonedCartTracking);
        }

        public async Task DeleteAbandonedCartTrackingAsync(AbandonedCartTracking abandonedCartTracking)
        {
            await _abandonedCartTrackingRepository.DeleteAsync(abandonedCartTracking);
        }

        public virtual async Task<IList<AbandonedCartTracking>> GetAbandonedCartTrackingsToBeQueuedAsync(DateTime timeTobeCheckedBefore)
        {
            var query = from u in _abandonedCartTrackingRepository.Table
                        where u.LastModifiedOnUtc < timeTobeCheckedBefore && u.IsQueued == false
                        orderby u.Id
                        select u;

            return await query.ToListAsync();
        }

        #endregion
    }
}
