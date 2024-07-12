using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models
{
    public record FAQCategorySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQCategories.List.SearchName")]
        public string SearchName { get; set; }
    }
}