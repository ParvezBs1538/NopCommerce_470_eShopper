using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models
{
    public partial record SEOTemplateLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOTitleTemplate")]
        public string SEOTitleTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEODescriptionTemplate")]
        public string SEODescriptionTemplate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOKeywordsTemplate")]
        public string SEOKeywordsTemplate { get; set; }

    }
}
