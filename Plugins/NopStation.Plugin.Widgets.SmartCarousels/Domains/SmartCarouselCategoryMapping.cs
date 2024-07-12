using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarouselCategoryMapping : BaseEntity
{
    public int CategoryId { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
