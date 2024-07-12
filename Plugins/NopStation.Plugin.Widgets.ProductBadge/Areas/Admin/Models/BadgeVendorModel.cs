using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeVendorModel : BaseNopEntityModel
{
    public int VendorId { get; set; }

    public string VendorName { get; set; }

    public bool Active { get; set; }
}