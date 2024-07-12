using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.EnableAjaxLoad")]
    public bool EnableAjaxLoad { get; set; }
    public bool EnableAjaxLoad_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.CacheActiveBadges")]
    public bool CacheActiveBadges { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.ProductDetailsWidgetZone")]
    public string ProductDetailsWidgetZone { get; set; }
    public bool ProductDetailsWidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.ProductBoxWidgetZone")]
    public string ProductBoxWidgetZone { get; set; }
    public bool ProductBoxWidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.SmallBadgeWidth")]
    public int SmallBadgeWidth { get; set; }
    public bool SmallBadgeWidth_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.MediumBadgeWidth")]
    public int MediumBadgeWidth { get; set; }
    public bool MediumBadgeWidth_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.LargeBadgeWidth")]
    public int LargeBadgeWidth { get; set; }
    public bool LargeBadgeWidth_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Configuration.Fields.IncreaseWidthInDetailsPageByPercentage")]
    public decimal IncreaseWidthInDetailsPageByPercentage { get; set; }
    public bool IncreaseWidthInDetailsPageByPercentage_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}