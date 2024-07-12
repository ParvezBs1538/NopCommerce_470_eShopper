using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public partial record AjaxFilterSpecificationAttributeSearchModel : BaseSearchModel
    {
        //public string SpecificiationAttributeName { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.SearchSpecificationAttributeName")]
        public string SearchSpecificationAttributeName { get; set; }
    }
}
