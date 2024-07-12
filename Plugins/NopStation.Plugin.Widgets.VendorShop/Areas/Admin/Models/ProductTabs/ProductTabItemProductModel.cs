using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.ProductTabs
{
    public record ProductTabItemProductModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.ProductTabItem")]
        public int ProductTabItemId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
