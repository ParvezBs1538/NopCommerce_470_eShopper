using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.WebApi.Services.Caching
{
    public partial class LocaleStringResourceCacheEventConsumer : CacheEventConsumer<LocaleStringResource>
    {
        protected override async Task ClearCacheAsync(LocaleStringResource entity)
        {
            await RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }
    }
}
