using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.Flipbooks.Services.Cache
{
    public partial class FlipbookCacheEventConsumer : CacheEventConsumer<Flipbook>
    {
        protected override async Task ClearCacheAsync(Flipbook entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<Flipbook>.Prefix);
            await RemoveByPrefixAsync(FlipbooksCacheDefaults.FlipbookPrefix);
        }
    }
}