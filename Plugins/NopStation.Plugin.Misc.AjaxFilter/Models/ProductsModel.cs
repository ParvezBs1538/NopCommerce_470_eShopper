using System.Collections.Generic;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public record ProductsModel : BasePageableModel
    {
        public ProductsModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public int Count { get; set; }
        public string WarningMessage { get; set; }
        public string NoResultMessage { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
        public string ViewMode { get; set; }
    }
}
