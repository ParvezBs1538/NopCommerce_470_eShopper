using Nop.Core;

namespace NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop
{
    public partial class OCarouselItem : BaseEntity
    {
        public int ProductId { get; set; }

        public int OCarouselId { get; set; }

        public int DisplayOrder { get; set; }

        public virtual OCarousel OCarousel { get; set; }
    }
}
