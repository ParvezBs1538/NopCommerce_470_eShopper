using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.VendorShop.Domains
{
    public class VendorProfile : BaseEntity, ILocalizedEntity
    {
        public int VendorId { get; set; }
        public string Description { get; set; }
        public string CustomCss { get; set; }
        public int ProfilePictureId { get; set; }
        public int BannerPictureId { get; set; }
        public int MobileBannerPictureId { get; set; }
        public int StoreId { get; set; }
    }
}
