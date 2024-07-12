using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Payments.Affirm.Domain;
using NopStation.Plugin.Payments.Affirm.Services.Cache;

namespace NopStation.Plugin.Payments.Affirm.Services
{
    public class AffirmPaymentTransactionService : IAffirmPaymentTransactionService
    {
        #region Fields

        private readonly IRepository<AffirmPaymentTransaction> _affirmPaymentTransactionRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public AffirmPaymentTransactionService(IRepository<AffirmPaymentTransaction> affirmPaymentTransactionRepository,
            IStaticCacheManager staticCacheManager)
        {
            _affirmPaymentTransactionRepository = affirmPaymentTransactionRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<AffirmPaymentTransaction>> GetAllTransactionsAsync(DateTime? fromDate = null, DateTime? toDate = null,
                int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffirmPaymentCacheDefaults.AffirmTransactionsAllKey,
                fromDate, toDate, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _affirmPaymentTransactionRepository.Table;

                if (fromDate.HasValue)
                    query = query.Where(x => x.CreatedOnUtc >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.CreatedOnUtc <= toDate.Value);

                query = query.OrderByDescending(x => x.CreatedOnUtc);

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task<AffirmPaymentTransaction> GetTransactionByReferenceAsync(Guid reference)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffirmPaymentCacheDefaults.AffirmTransactionByReferenceKey, reference);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
                await _affirmPaymentTransactionRepository.Table.Where(x => x.OrderGuid == reference).FirstOrDefaultAsync());
        }

        public virtual async Task InsertTransactionAsync(AffirmPaymentTransaction affirmTransaction)
        {
            await _affirmPaymentTransactionRepository.InsertAsync(affirmTransaction);
        }

        public virtual async Task UpdateTransactionAsync(AffirmPaymentTransaction affirmTransaction)
        {
            await _affirmPaymentTransactionRepository.UpdateAsync(affirmTransaction);
        }

        public virtual async Task DeleteTransactionAsync(AffirmPaymentTransaction affirmTransaction)
        {
            await _affirmPaymentTransactionRepository.DeleteAsync(affirmTransaction);
        }

        #endregion
    }
}
