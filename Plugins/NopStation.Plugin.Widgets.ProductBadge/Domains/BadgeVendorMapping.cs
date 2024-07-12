using Nop.Core;

namespace NopStation.Plugin.Widgets.ProductBadge.Domains;

public class BadgeVendorMapping : BaseEntity
{
    public int BadgeId { get; set; }

    public int VendorId { get; set; }
}