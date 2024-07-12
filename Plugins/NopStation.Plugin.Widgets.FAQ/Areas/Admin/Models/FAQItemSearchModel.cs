using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models
{
    public record FAQItemSearchModel : BaseSearchModel
    {
        public FAQItemSearchModel()
        {
            AvailableFAQCategories = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.List.SearchKeyword")]
        public string SearchKeyword { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FAQ.FAQItems.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableFAQCategories { get; set; }
    }
}