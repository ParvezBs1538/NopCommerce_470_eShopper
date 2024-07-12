using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Tax.TaxJar.Domains;

namespace NopStation.Plugin.Tax.TaxJar.Services.Cache
{
    public partial class TaxJarCategoryCacheEventConsumer : CacheEventConsumer<TaxJarCategory>
    {
        protected override async Task ClearCacheAsync(TaxJarCategory entity)
        {
            await RemoveByPrefixAsync(TaxJarCacheDefaults.CategoryPrefix);
        }
    }
}