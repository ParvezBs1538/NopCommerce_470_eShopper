using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductPdf.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductPdf.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductPdf.Configuration.Fields.LetterPageSizeEnabled")]
        public bool LetterPageSizeEnabled { get; set; }
        public bool LetterPageSizeEnabled_OverrideForStore { get; set; }
    }
}
