using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Services.Caching;
namespace NopStation.Plugin.Widgets.DMS.Services.Cache
{
    public partial class LocaleStringResourceCacheEventConsumer : CacheEventConsumer<LocaleStringResource>
    {
        protected override async Task ClearCacheAsync(LocaleStringResource entity)
        {
            await RemoveByPrefixAsync(DMSCacheDefaults.StringResourcePrefixCacheKey);
        }
    }
}
