using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarouselProductMapping : BaseEntity
{
    public int ProductId { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
