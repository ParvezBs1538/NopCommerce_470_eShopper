using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Payments.Affirm.Domain;

namespace NopStation.Plugin.Payments.Affirm.Services.Cache
{
    public partial class AffirmPaymentTransactionCacheEventConsumer : CacheEventConsumer<AffirmPaymentTransaction>
    {
        protected override async Task ClearCacheAsync(AffirmPaymentTransaction entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<AffirmPaymentTransaction>.Prefix);
            await RemoveByPrefixAsync(AffirmPaymentCacheDefaults.AffirmTransactionPrefix);
        }
    }
}