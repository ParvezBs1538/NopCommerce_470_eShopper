using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop
{
    public class SliderItem : BaseEntity, ILocalizedEntity
    {
        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public int PictureId { get; set; }

        public int MobilePictureId { get; set; }

        public string ImageAltText { get; set; }

        public string Link { get; set; }

        public string ShopNowLink { get; set; }

        public int SliderId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
