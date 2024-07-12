using Nop.Core;

namespace NopStation.Plugin.Widgets.ProductBadge.Domains;

public class BadgeProductMapping : BaseEntity
{
    public int BadgeId { get; set; }

    public int ProductId { get; set; }
}