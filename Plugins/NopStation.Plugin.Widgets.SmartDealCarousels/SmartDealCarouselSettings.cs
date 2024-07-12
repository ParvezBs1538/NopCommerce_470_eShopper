using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SmartDealCarousels;

public class SmartDealCarouselSettings : ISettings
{
    public bool EnableCarousel { get; set; }

    public bool EnableAjaxLoad { get; set; }

    public int CarouselPictureSize { get; set; }
}
