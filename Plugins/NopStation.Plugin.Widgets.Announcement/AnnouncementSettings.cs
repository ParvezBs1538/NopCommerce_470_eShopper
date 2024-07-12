using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.Announcement;

public class AnnouncementSettings : ISettings
{
    public bool EnablePlugin { get; set; }

    public string WidgetZone { get; set; }

    public string ItemSeparator { get; set; }

    public int DisplayTypeId { get; set; }

    public bool AllowCustomersToMinimize { get; set; }

    public bool AllowCustomersToClose { get; set; }
}