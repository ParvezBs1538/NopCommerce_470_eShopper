using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

public class PrevNextProductSettings : ISettings
{
    public bool EnableLoop { get; set; }

    public string WidgetZone { get; set; }

    public int NavigateBasedOnId { get; set; }

    public int ProductNameMaxLength { get; set; }
}
