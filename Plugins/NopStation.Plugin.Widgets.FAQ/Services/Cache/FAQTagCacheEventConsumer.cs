using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services.Cache
{
    public partial class FAQTagCacheEventConsumer : CacheEventConsumer<FAQTag>
    {
        protected override async Task ClearCacheAsync(FAQTag entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<FAQTag>.Prefix);
        }
    }
}