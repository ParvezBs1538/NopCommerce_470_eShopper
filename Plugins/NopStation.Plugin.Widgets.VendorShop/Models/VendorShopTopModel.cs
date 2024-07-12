using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{
    public record VendorShopTopModel : BaseNopModel
    {
        public string BannerPictureUrl { get; set; }
    }
}
