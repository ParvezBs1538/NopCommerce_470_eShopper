using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public record AjaxFilterParentCategoryListSearchModel : BaseSearchModel
    {
        public AjaxFilterParentCategoryListSearchModel()
        {
            AvaliableCategoryIds = new List<int>();
        }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public IList<int> AvaliableCategoryIds;
    }
}
