using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{

    public record VendorShopHeaderModel : BaseNopModel
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public bool EnableSubscribeFeature { get; set; }
        public bool AllowCustomersToContactVendors { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string BannerPictureUrl { get; set; }
        public string MobileBannerPictureUrl { get; set; }
        public bool IsCurrentCustomerSubscribed { get; set; }
        public bool IsRegisteredCustomer { get; set; }
    }
}
