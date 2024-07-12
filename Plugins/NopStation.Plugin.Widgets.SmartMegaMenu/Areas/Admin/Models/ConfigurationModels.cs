using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.Configuration.Fields.EnableMegaMenu")]
    public bool EnableMegaMenu { get; set; }
    public bool EnableMegaMenu_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.Configuration.Fields.HideDefaultMenu")]
    public bool HideDefaultMenu { get; set; }
    public bool HideDefaultMenu_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.Configuration.Fields.MenuItemPictureSize")]
    public int MenuItemPictureSize { get; set; }
    public bool MenuItemPictureSize_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}