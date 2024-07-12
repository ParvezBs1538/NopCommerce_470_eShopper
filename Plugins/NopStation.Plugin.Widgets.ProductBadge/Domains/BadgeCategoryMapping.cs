using Nop.Core;

namespace NopStation.Plugin.Widgets.ProductBadge.Domains;

public class BadgeCategoryMapping : BaseEntity
{
    public int BadgeId { get; set; }

    public int CategoryId { get; set; }
}