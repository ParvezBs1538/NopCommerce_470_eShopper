using System;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Models;

public record CarouselOverviewModel : BaseNopEntityModel
{
    public CarouselOverviewModel()
    {
        Picture = new PictureModel();
    }

    public string Title { get; set; }

    public bool DisplayTitle { get; set; }
    public string CustomUrl { get; set; }
    public bool ShowCarouselPicture { get; set; }
    public PictureModel Picture { get; set; }
    public PositionType PicturePosition { get; set; }
    public bool ShowBackground { get; set; }
    public DateTime? CountdownUntill { get; set; }
    public string BackgroundPictureUrl { get; set; }
    public string BackgroundColor { get; set; }
    public bool ShowCountdown { get; set; }
    public string CustomCssClass { get; set; }
    public BackgroundType BackgroundType { get; set; }
}
