using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.FAQ.Domains;

namespace NopStation.Plugin.Widgets.FAQ.Services.Cache
{
    public partial class FAQCategoryCacheEventConsumer : CacheEventConsumer<FAQCategory>
    {
        protected override async Task ClearCacheAsync(FAQCategory entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<FAQCategory>.Prefix);
        }
    }
}