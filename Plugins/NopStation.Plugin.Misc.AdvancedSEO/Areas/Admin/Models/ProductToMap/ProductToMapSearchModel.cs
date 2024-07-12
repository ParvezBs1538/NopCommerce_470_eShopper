using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductToMap
{
    public partial record ProductToMapSearchModel : BaseSearchModel
    {
        public ProductToMapSearchModel()
        {
            SelectedProductIds = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMapSearch.Fields.ProductName")]
        public string ProductName { get; set; }

        public int ProductSEOTemplateId { get; set; }

        public IList<int> SelectedProductIds { get; set; }
    }
}
