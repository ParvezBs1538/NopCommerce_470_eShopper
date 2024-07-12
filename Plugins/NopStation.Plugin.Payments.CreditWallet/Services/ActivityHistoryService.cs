using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public class ActivityHistoryService : IActivityHistoryService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ActivityHistory> _activityHistoryRepository;

        public ActivityHistoryService(IRepository<Customer> customerRepository,
            IRepository<ActivityHistory> activityHistoryRepository)
        {
            _customerRepository = customerRepository;
            _activityHistoryRepository = activityHistoryRepository;
        }

        public async Task DeleteActivityHistoryAsync(ActivityHistory activityHistory)
        {
            await _activityHistoryRepository.InsertAsync(activityHistory);
        }

        public async Task<IList<ActivityHistory>> GetWalletActivityHistoryAsync(Wallet wallet)
        {
            return await GetWalletActivityHistoryAsync(wallet.WalletCustomerId);
        }

        public async Task<IList<ActivityHistory>> GetWalletActivityHistoryAsync(int walletCustomerId)
        {
            var query = from c in _activityHistoryRepository.Table
                        orderby c.CreatedOnUtc descending
                        where c.WalletCustomerId == walletCustomerId
                        select c;

            return await query.ToListAsync();
        }

        public async Task InsertActivityHistoryAsync(ActivityHistory activityHistory)
        {
            await _activityHistoryRepository.InsertAsync(activityHistory);
        }

        public async Task UpdateActivityHistoryAsync(ActivityHistory activityHistory)
        {
            await _activityHistoryRepository.UpdateAsync(activityHistory);
        }

        public async Task<IPagedList<ActivityHistory>> GetAllActivityHistoryAsync(int customerId = 0,
            string email = "", int[] atids = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from ah in _activityHistoryRepository.Table
                        join c in _customerRepository.Table on ah.WalletCustomerId equals c.Id
                        where (atids == null || !atids.Any() || atids.Contains(ah.ActivityTypeId)) &&
                            (createdFromUtc == null || ah.CreatedOnUtc.Date >= createdFromUtc.Value.Date) &&
                            (createdToUtc == null || ah.CreatedOnUtc.Date <= createdToUtc.Value.Date)
                        select new { ActivityHistory = ah, Customer = c };

            if (customerId > 0)
                query = query.Where(x => x.Customer.Id == customerId);
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(x => x.Customer.Email.Contains(email));

            var activityQuery = query.Select(x => x.ActivityHistory).Distinct().OrderByDescending(x => x.CreatedOnUtc);

            return await activityQuery.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
