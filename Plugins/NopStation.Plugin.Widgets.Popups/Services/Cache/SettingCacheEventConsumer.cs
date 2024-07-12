using System.Threading.Tasks;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.Popups.Services.Cache;

public partial class SettingCacheEventConsumer : CacheEventConsumer<Setting>
{
    protected override async Task ClearCacheAsync(Setting entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Name))
            return;

        if (entity.Name.Equals(nameof(CustomerSettings.NewsletterBlockAllowToUnsubscribe), System.StringComparison.InvariantCultureIgnoreCase))
            await RemoveByPrefixAsync(PopupCacheDefaults.DefaultPopupModelPrefix);
    }
}