using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselManufacturerSearchModel : BaseSearchModel
{
    public int CarouselId { get; set; }
}
