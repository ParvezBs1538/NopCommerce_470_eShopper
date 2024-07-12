using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Configuration.Fields.EnableSlider")]
    public bool EnableSlider { get; set; }
    public bool EnableSlider_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Configuration.Fields.EnableAjaxLoad")]
    public bool EnableAjaxLoad { get; set; }
    public bool EnableAjaxLoad_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Configuration.Fields.SupportedVideoExtensions")]
    public string SupportedVideoExtensions { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}