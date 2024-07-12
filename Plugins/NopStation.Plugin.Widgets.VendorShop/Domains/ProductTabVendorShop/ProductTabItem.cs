using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop
{
    public class ProductTabItem : BaseEntity, ILocalizedEntity
    {
        private ICollection<ProductTabItemProduct> _productTabItemProducts;

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public int ProductTabId { get; set; }

        public virtual ProductTab ProductTab { get; set; }

        public virtual ICollection<ProductTabItemProduct> ProductTabItemProducts
        {
            get => _productTabItemProducts ??= new List<ProductTabItemProduct>();
            set => _productTabItemProducts = value;
        }
    }
}
