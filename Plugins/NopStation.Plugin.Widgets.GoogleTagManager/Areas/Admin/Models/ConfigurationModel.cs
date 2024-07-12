using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleTagManager.Configuration.Fields.IsEnable")]
        public bool IsEnable { get; set; }
        public bool IsEnable_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.GoogleTagManager.Configuration.Fields.GTMContainerId")]
        public string GTMContainerId { get; set; }
        public bool GTMContainerId_OverrideForStore { get; set; }

        #endregion
    }
}
