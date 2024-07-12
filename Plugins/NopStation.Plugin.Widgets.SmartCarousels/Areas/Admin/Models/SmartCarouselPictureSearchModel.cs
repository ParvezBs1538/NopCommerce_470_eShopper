using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselPictureSearchModel : BaseSearchModel
{
    public int CarouselId { get; set; }
}
