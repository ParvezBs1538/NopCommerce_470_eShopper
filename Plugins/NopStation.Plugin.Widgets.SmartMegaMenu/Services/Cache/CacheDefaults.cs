using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Services.Cache;

public class CacheDefaults
{
    public static CacheKey MegaMenuItemsByParentIdKey => new CacheKey("Nopstation.smartmegamenu.items-byparentid-{0}-{1}", MegaMenuItemsPrefix);
    public static CacheKey MegaMenuItemsKey => new CacheKey("Nopstation.smartmegamenu.items-{0}-{1}-{2}", MegaMenuItemsPrefix);
    public static string MegaMenuItemsPrefix => "Nopstation.smartmegamenu.items-{0}";
}