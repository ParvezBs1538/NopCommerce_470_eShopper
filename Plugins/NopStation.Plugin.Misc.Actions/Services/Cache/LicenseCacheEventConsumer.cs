using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Misc.Core.Domains;

namespace NopStation.Plugin.Misc.Core.Services.Cache;

public partial class LicenseCacheEventConsumer : CacheEventConsumer<License>
{
    protected override async Task ClearCacheAsync(License entity)
    {
        await RemoveByPrefixAsync(NopStationEntityCacheDefaults<License>.Prefix);
        await RemoveByPrefixAsync(CoreCacheDefaults.LicensePrefix);
    }
}