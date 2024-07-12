using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Caching;

namespace NopStation.Plugin.Tax.TaxJar.Services.Cache
{
    public class CustomerCacheEventConsumer : CacheEventConsumer<Customer>
    {
        #region Methods

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Customer entity)
        {
            await RemoveByPrefixAsync(TaxJarDefaults.TaxRateCacheKeyByCustomerPrefix, entity);
            await base.ClearCacheAsync(entity);
        }

        #endregion
    }
}
