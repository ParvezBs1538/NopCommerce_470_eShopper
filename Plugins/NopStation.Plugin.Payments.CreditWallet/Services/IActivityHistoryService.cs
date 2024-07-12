using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public interface IActivityHistoryService
    {
        Task InsertActivityHistoryAsync(ActivityHistory activityHistory);

        Task UpdateActivityHistoryAsync(ActivityHistory activityHistory);

        Task DeleteActivityHistoryAsync(ActivityHistory activityHistory);

        Task<IList<ActivityHistory>> GetWalletActivityHistoryAsync(Wallet wallet);

        Task<IList<ActivityHistory>> GetWalletActivityHistoryAsync(int walletCustomerId);

        Task<IPagedList<ActivityHistory>> GetAllActivityHistoryAsync(int customerId = 0,
            string email = "", int[] atids = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
