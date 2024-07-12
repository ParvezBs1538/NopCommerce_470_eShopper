using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProductBadge;

public partial class ProductBadgeSettings : ISettings
{
    public bool EnableAjaxLoad { get; set; }
    public bool CacheActiveBadges { get; set; }
    public string ProductDetailsWidgetZone { get; set; }
    public string ProductBoxWidgetZone { get; set; }

    public int SmallBadgeWidth { get; set; }
    public int MediumBadgeWidth { get; set; }
    public int LargeBadgeWidth { get; set; }
    public decimal IncreaseWidthInDetailsPageByPercentage { get; set; }
}