using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public class TaxjarTransactionLogService
    {
        #region Fields

        private readonly IRepository<TaxjarTransactionLog> _taxjarTransactionLogRepository;

        #endregion

        #region Ctor
        public TaxjarTransactionLogService(IRepository<TaxjarTransactionLog> taxjarTransactionLogRepository)
        {
            _taxjarTransactionLogRepository = taxjarTransactionLogRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<TaxjarTransactionLog>> GetTaxjarTransactionLogAsync(int? customerId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var logs = await _taxjarTransactionLogRepository.GetAllPagedAsync(query =>
            {
                if (customerId.HasValue)
                    query = query.Where(logItem => logItem.CustomerId == customerId);
                if (createdFromUtc.HasValue)
                    query = query.Where(logItem => logItem.CreatedDateUtc >= createdFromUtc.Value);
                if (createdToUtc.HasValue)
                    query = query.Where(logItem => logItem.CreatedDateUtc <= createdToUtc.Value);
                query = query.OrderByDescending(logItem => logItem.CreatedDateUtc).ThenByDescending(logItem => logItem.Id);

                return query;
            }, pageIndex, pageSize);

            return logs;
        }

        public virtual async Task<TaxjarTransactionLog> GetTaxjarTransactionLogByIdAsync(int logItemId)
        {
            if (logItemId == 0)
                return null;

            return await _taxjarTransactionLogRepository.GetByIdAsync(logItemId);
        }

        public virtual async Task<TaxjarTransactionLog> GetTaxjarTransactionLogByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
                return null;

            return await _taxjarTransactionLogRepository.Table.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
        }
        public virtual async Task InsertTaxjarTransactionLogAsync(TaxjarTransactionLog logItem)
        {
            if (logItem == null)
                throw new ArgumentNullException(nameof(logItem));

            await _taxjarTransactionLogRepository.InsertAsync(logItem);
        }

        public virtual async Task UpdateTaxjarTransactionLogAsync(TaxjarTransactionLog logItem)
        {
            if (logItem == null)
                throw new ArgumentNullException(nameof(logItem));

            await _taxjarTransactionLogRepository.UpdateAsync(logItem);
        }

        public virtual async Task DeleteTaxjarTransactionLogAsync(TaxjarTransactionLog logItem)
        {
            if (logItem == null)
                throw new ArgumentNullException(nameof(logItem));

            await _taxjarTransactionLogRepository.DeleteAsync(logItem);
        }

        public virtual async Task DeleteTaxjarTransactionLogAsync(int[] ids)
        {
            await _taxjarTransactionLogRepository.DeleteAsync(logItem => ids.Contains(logItem.Id));
        }


        #endregion
    }
}
