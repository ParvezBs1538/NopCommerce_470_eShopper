using Nop.Web.Framework.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Models;

public record SliderOverviewModel : BaseNopEntityModel
{
    public bool ShowBackground { get; set; }
    public string BackgroundPictureUrl { get; set; }
    public string BackgroundColor { get; set; }
    public string CustomCssClass { get; set; }
    public BackgroundType BackgroundType { get; set; }
}
