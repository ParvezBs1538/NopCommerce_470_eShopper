using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.WebApi.Services.Caching
{
    public partial class ApiStringResourceCacheEventConsumer : CacheEventConsumer<ApiStringResource>
    {
        protected override async Task ClearCacheAsync(ApiStringResource entity)
        {
            await RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }
    }
}
