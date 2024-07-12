using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductTabs.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.ProductTabs.Configuration.Fields.EnableProductTab")]
        public bool EnableProductTab { get; set; }
        public bool EnableProductTab_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}