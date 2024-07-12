using Nop.Core;

namespace NopStation.Plugin.Widgets.ProductBadge.Domains;

public class BadgeManufacturerMapping : BaseEntity
{
    public int BadgeId { get; set; }

    public int ManufacturerId { get; set; }
}