using System.Threading.Tasks;
using NopStation.Plugin.Payments.MPay24.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Payments.MPay24.Services.Cache
{
    public partial class PaymentOptionCacheEventConsumer : CacheEventConsumer<PaymentOption>
    {
        protected override async Task ClearCacheAsync(PaymentOption entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<PaymentOption>.Prefix);
            await RemoveByPrefixAsync(MPay24CacheDefaults.PaymentOptionPrefix);
        }
    }
}