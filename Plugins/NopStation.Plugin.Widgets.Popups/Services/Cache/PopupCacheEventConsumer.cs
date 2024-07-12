using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Services.Cache;

public partial class PopupCacheEventConsumer : CacheEventConsumer<Popup>
{
    protected override async Task ClearCacheAsync(Popup entity)
    {
        await RemoveByPrefixAsync(PopupCacheDefaults.PopupModelPrefix, entity.Id);
    }
}