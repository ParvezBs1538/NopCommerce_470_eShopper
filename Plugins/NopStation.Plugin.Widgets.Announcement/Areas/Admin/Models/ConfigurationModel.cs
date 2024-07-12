using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.EnablePlugin")]
    public bool EnablePlugin { get; set; }
    public bool EnablePlugin_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.WidgetZone")]
    public string WidgetZone { get; set; }
    public bool WidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.ItemSeparator")]
    public string ItemSeparator { get; set; }
    public bool ItemSeparator_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.DisplayType")]
    public int DisplayTypeId { get; set; }
    public bool DisplayTypeId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToMinimize")]
    public bool AllowCustomersToMinimize { get; set; }
    public bool AllowCustomersToMinimize_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.Configuration.Fields.AllowCustomersToClose")]
    public bool AllowCustomersToClose { get; set; }
    public bool AllowCustomersToClose_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
