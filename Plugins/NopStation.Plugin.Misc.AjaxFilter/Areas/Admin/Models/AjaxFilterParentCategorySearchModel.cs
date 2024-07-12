using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public record AjaxFilterParentCategorySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AjaxFilter.SearchParentCategoryName")]
        public string SearchParentCategoryName { get; set; }
    }
}
