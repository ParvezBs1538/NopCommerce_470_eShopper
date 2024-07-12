using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SmartCarousels;

public class SmartCarouselSettings : ISettings
{
    public bool EnableCarousel { get; set; }

    public bool EnableAjaxLoad { get; set; }
}
