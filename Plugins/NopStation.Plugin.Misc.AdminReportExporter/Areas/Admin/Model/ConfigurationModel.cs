using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Model
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.AdminReportExporter.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
