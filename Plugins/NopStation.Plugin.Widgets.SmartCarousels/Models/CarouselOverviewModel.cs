using Nop.Web.Framework.Models;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels.Models;

public record CarouselOverviewModel : BaseNopEntityModel
{
    public string Title { get; set; }

    public bool DisplayTitle { get; set; }
    public string CustomUrl { get; set; }
    public bool ShowBackground { get; set; }
    public string BackgroundPictureUrl { get; set; }
    public string BackgroundColor { get; set; }
    public string CustomCssClass { get; set; }
    public BackgroundType BackgroundType { get; set; }
    public CarouselType CarouselType { get; set; }
}
