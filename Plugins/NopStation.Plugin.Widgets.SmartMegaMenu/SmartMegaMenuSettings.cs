using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SmartMegaMenu;

public class SmartMegaMenuSettings : ISettings
{
    public bool EnableMegaMenu { get; set; }

    public bool HideDefaultMenu { get; set; }

    public int MenuItemPictureSize { get; set; }
}
