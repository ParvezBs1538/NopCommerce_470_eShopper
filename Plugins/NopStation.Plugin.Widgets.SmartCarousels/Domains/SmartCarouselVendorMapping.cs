using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarouselVendorMapping : BaseEntity
{
    public int VendorId { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
