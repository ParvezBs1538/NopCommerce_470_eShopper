using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.Flipbooks.Services.Cache
{
    public partial class FlipbookContentProductCacheEventConsumer : CacheEventConsumer<FlipbookContentProduct>
    {
        protected override async Task ClearCacheAsync(FlipbookContentProduct entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<FlipbookContentProduct>.Prefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookContentProductPrefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookContentPrefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookPrefix);
        }
    }
}