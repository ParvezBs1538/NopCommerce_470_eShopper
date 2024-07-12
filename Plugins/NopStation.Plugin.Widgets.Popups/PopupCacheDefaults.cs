using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.Popups;

public class PopupCacheDefaults
{
    public static CacheKey PopupModelCacheKey => new CacheKey("Nopstation.popups.popupmodel.byid.{0}-{1}-{2}-{3}-{4}", PopupModelPrefix);

    public static string PopupModelPrefix => "Nopstation.popups.popupmodel.byid.{0}";

    public static CacheKey DefaultPopupModelCacheKey => new CacheKey("Nopstation.popups.defaultpopupmodel.byid.{0}-{1}-{2}", DefaultPopupModelPrefix);

    public static string DefaultPopupModelPrefix => "Nopstation.popups.defaultpopupmodel.";
}