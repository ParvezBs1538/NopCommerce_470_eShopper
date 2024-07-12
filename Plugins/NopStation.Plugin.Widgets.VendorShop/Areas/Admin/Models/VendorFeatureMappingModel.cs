using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models
{
    public record VendorFeatureMappingModel : BaseNopEntityModel
    {
        public int VendorId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.VendorShop.VendorFeature.Fields.EnableFeature")]
        public bool VendorShopEnable { get; set; }
    }
}
