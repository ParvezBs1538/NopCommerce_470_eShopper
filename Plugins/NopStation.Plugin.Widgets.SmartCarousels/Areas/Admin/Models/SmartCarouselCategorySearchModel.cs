using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselCategorySearchModel : BaseSearchModel
{
    public int CarouselId { get; set; }
}
