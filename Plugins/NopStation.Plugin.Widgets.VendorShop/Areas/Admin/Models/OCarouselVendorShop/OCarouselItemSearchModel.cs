using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.OCarouselVendorShop
{
    public record OCarouselItemSearchModel : BaseSearchModel
    {
        public int OCarouselId { get; set; }
    }
}
