using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

public partial class SmartDealCarouselProductMapping : BaseEntity
{
    public int ProductId { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
