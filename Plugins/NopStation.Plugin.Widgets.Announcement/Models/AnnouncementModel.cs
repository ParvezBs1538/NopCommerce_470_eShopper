using System.Collections.Generic;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Models;

public class AnnouncementModel
{
    public AnnouncementModel()
    {
        AnnouncementItems = new List<AnnouncementItem>();
    }

    public string WidgetZone { get; set; }

    public string ItemSeparator { get; set; }

    public DisplayType DisplayType { get; set; }

    public bool AllowCustomersToMinimize { get; set; }

    public bool AllowCustomersToClose { get; set; }

    public bool AnnouncementBarMinimized { get; set; }

    public IList<AnnouncementItem> AnnouncementItems { get; set; }


    public record AnnouncementItem : BaseNopEntityModel
    {
        public int DisplayOrder { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Color { get; set; }
    }
}
