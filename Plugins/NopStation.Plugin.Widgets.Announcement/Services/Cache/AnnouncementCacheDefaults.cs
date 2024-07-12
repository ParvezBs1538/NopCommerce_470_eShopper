using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.Announcement.Services.Cache;

public class AnnouncementCacheDefaults
{
    public static CacheKey AnnouncementItemsAllKey => new CacheKey("Nopstation.announcement.announcementitems.all.{0}-{1}-{2}-{3}-{4}", AnnouncementItemPrefix);
    public static string AnnouncementItemPrefix => "Nopstation.announcement.announcementitems.";
}