using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselVendorModel : BaseNopEntityModel
{
    public int CarouselId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Vendors.Fields.Vendor")]
    public int VendorId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Vendors.Fields.Vendor")]
    public string VendorName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Vendors.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
}
