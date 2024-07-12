using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop
{
    public record OCarouselItemModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.OCarousel")]
        public int OCarouselId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.Picture")]
        public string PictureUrl { get; set; }
    }
}
