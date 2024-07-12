using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.Dmoney.Domains;

namespace NopStation.Plugin.Payments.Dmoney.Services
{
    public interface IDmoneyTransactionService
    {
        Task<IPagedList<DmoneyTransaction>> GetAllTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null,
            int orderId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<DmoneyTransaction> GetTransactionByTrackingNumberAsync(string transactionTrackingNo);

        Task InsertTransactionAsync(DmoneyTransaction dmoneyTransaction);

        Task UpdateTransactionAsync(DmoneyTransaction dmoneyTransaction);

        Task DeleteTransactionAsync(DmoneyTransaction dmoneyTransaction);
    }
}