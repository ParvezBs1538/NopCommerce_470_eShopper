using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Infrastructure.Cache;

public class CacheDefaults
{
    public static CacheKey MegaMenuItemPictureModelKey => new CacheKey("Nopstation.smartmegamenu.menuitems.picture-{0}-{1}-{2}-{3}-{4}", MegaMenuItemPicturePrefix);
    public static string MegaMenuItemPicturePrefix => "Nopstation.smartmegamenu.menuitems.picture-{0}";
}