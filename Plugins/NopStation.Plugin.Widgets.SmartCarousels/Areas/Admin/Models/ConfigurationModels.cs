using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Configuration.Fields.EnableCarousel")]
    public bool EnableCarousel { get; set; }
    public bool EnableCarousel_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Configuration.Fields.EnableAjaxLoad")]
    public bool EnableAjaxLoad { get; set; }
    public bool EnableAjaxLoad_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}