using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.Flipbooks.Services.Cache
{
    public partial class FlipbookContentCacheEventConsumer : CacheEventConsumer<FlipbookContent>
    {
        protected override async Task ClearCacheAsync(FlipbookContent entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<FlipbookContent>.Prefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookContentPrefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookPrefix);
        }
    }
}