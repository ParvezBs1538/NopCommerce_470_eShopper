using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IAbandonedCartTrackingService
    {
        Task<IPagedList<AbandonedCartTracking>> GetAllAbandonedCartTrackingsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<AbandonedCartTracking> GetAbandonedCartTrackingByCustomerAsync(int customerId);

        Task InsertAbandonedCartTrackingAsync(AbandonedCartTracking smartGroup);

        Task<AbandonedCartTracking> GetAbandonedCartTrackingByIdAsync(int id);

        Task UpdateAbandonedCartTrackingAsync(AbandonedCartTracking smartGroup);

        Task DeleteAbandonedCartTrackingAsync(AbandonedCartTracking smartGroup);

        Task<IList<AbandonedCartTracking>> GetAbandonedCartTrackingsToBeQueuedAsync(DateTime timeTobeCheckedBefore);
    }
}
