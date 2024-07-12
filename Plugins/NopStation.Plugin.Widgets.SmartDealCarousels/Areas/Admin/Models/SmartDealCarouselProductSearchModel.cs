using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;

public record SmartDealCarouselProductSearchModel : BaseSearchModel
{
    public int CarouselId { get; set; }
}
