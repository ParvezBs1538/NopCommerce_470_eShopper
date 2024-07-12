using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Payments.Dmoney.Domains;

namespace NopStation.Plugin.Payments.Dmoney.Services
{
    public class DmoneyTransactionService : IDmoneyTransactionService
    {
        #region Fields

        private readonly IRepository<DmoneyTransaction> _dmoneyTransactionRepository;

        #endregion

        #region Ctor

        public DmoneyTransactionService(IRepository<DmoneyTransaction> dmoneyTransactionRepository)
        {
            _dmoneyTransactionRepository = dmoneyTransactionRepository;
        }

        #endregion

        public virtual async Task<IPagedList<DmoneyTransaction>> GetAllTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null,
            int orderId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _dmoneyTransactionRepository.Table;

            if (fromDate.HasValue)
                query = query.Where(x => x.CreatedOnUtc >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.CreatedOnUtc <= toDate.Value);

            if (orderId > 0)
                query = query.Where(x => x.OrderId == orderId);

            query = query.OrderByDescending(x => x.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<DmoneyTransaction> GetTransactionByTrackingNumberAsync(string transactionTrackingNo)
        {
            return await _dmoneyTransactionRepository.Table
                .Where(x => x.TransactionTrackingNumber == transactionTrackingNo).FirstOrDefaultAsync();
        }

        public virtual async Task InsertTransactionAsync(DmoneyTransaction dmoneyTransaction)
        {
            await _dmoneyTransactionRepository.InsertAsync(dmoneyTransaction);
        }

        public virtual async Task UpdateTransactionAsync(DmoneyTransaction dmoneyTransaction)
        {
            await _dmoneyTransactionRepository.UpdateAsync(dmoneyTransaction);
        }

        public virtual async Task DeleteTransactionAsync(DmoneyTransaction dmoneyTransaction)
        {
            await _dmoneyTransactionRepository.DeleteAsync(dmoneyTransaction);
        }
    }
}
