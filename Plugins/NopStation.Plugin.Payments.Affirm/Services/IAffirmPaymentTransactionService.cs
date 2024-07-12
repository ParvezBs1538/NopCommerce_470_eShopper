using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.Affirm.Domain;

namespace NopStation.Plugin.Payments.Affirm.Services
{
    public interface IAffirmPaymentTransactionService
    {
        Task DeleteTransactionAsync(AffirmPaymentTransaction affirmTransaction);

        Task<IPagedList<AffirmPaymentTransaction>> GetAllTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<AffirmPaymentTransaction> GetTransactionByReferenceAsync(Guid reference);

        Task InsertTransactionAsync(AffirmPaymentTransaction affirmTransaction);

        Task UpdateTransactionAsync(AffirmPaymentTransaction affirmTransaction);
    }
}
