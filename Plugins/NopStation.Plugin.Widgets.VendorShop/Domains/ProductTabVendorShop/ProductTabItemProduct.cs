using Nop.Core;

namespace NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop
{
    public class ProductTabItemProduct : BaseEntity
    {
        public int ProductTabItemId { get; set; }

        public int ProductId { get; set; }

        public int DisplayOrder { get; set; }

        public virtual ProductTabItem ProductTabItem { get; set; }
    }
}
