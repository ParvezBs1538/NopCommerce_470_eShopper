using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.VendorShop.Domains.ProductTabVendorShop
{
    public class ProductTab : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        private ICollection<ProductTabItem> _productTabItems;

        public string Name { get; set; }
        public int VendorId { get; set; }

        public string TabTitle { get; set; }

        public bool DisplayTitle { get; set; }

        public int PictureId { get; set; }

        public string PictureAlt { get; set; }

        public string PictureTitle { get; set; }

        public int DisplayOrder { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        public string CustomUrl { get; set; }

        public int WidgetZoneId { get; set; }

        public bool AutoPlay { get; set; }

        public string CustomCssClass { get; set; }

        public bool Loop { get; set; }

        public int Margin { get; set; }

        public int StartPosition { get; set; }

        public bool Center { get; set; }

        public bool Nav { get; set; }

        public bool LazyLoad { get; set; }

        public int LazyLoadEager { get; set; }

        public int AutoPlayTimeout { get; set; }

        public bool AutoPlayHoverPause { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool LimitedToStores { get; set; }

        public virtual ICollection<ProductTabItem> ProductTabItems
        {
            get => _productTabItems ??= new List<ProductTabItem>();
            set => _productTabItems = value;
        }
    }
}
