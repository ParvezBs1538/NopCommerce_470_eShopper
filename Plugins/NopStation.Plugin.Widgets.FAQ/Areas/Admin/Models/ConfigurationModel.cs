using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.FAQ.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.Configuration.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }
        public bool IncludeInTopMenu_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.Configuration.Fields.IncludeInFooter")]
        public bool IncludeInFooter { get; set; }
        public bool IncludeInFooter_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.Configuration.Fields.FooterElementSelector")]
        public string FooterElementSelector { get; set; }
        public bool FooterElementSelector_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
