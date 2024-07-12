using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarouselManufacturerMapping : BaseEntity
{
    public int ManufacturerId { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
