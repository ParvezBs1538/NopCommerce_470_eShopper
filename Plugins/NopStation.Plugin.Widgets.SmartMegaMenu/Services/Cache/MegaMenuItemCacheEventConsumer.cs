using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Services.Cache;

public partial class MegaMenuItemCacheEventConsumer : CacheEventConsumer<MegaMenuItem>
{
    protected override async Task ClearCacheAsync(MegaMenuItem entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.MegaMenuItemsPrefix, entity.MegaMenuId);
    }
}
