using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryToMap
{
    public partial record CategoryToMapSearchModel : BaseSearchModel
    {
        public CategoryToMapSearchModel()
        {
            SelectedCategoryIds = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMapSearch.Fields.CategoryName")]
        public string CategoryName { get; set; }

        public int CategorySEOTemplateId { get; set; }

        public IList<int> SelectedCategoryIds { get; set; }
    }
}
