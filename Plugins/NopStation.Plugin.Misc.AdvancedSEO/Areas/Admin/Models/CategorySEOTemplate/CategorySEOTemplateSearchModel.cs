using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate
{
    public partial record CategorySEOTemplateSearchModel : BaseSearchModel
    {
        public CategorySEOTemplateSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableTemplateType = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStatus")]
        public int SearchStatus { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchTemplateName")]
        public string SearchTemplateName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplates.List.SearchTemplateType")]
        public int SearchTemplateType { get; set; }

        public IList<SelectListItem> AvailableStatus { get; set; }

        public IList<SelectListItem> AvailableTemplateType { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public bool HideStoresList { get; set; }
    }
}
