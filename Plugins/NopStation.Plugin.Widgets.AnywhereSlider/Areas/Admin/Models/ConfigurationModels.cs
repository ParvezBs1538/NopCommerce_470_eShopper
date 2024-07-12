using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.AnywhereSlider.Configuration.Fields.EnableSlider")]
        public bool EnableSlider { get; set; }
        public bool EnableSlider_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}