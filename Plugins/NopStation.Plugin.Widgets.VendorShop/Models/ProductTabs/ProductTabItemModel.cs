using System.Collections.Generic;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.VendorShop.Models.ProductTabs
{
    public class ProductTabItemModel
    {
        public ProductTabItemModel()
        {
            Products = new List<ProductOverviewModel>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public List<ProductOverviewModel> Products { get; set; }
    }
}
