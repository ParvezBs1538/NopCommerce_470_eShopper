using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Services.Cache;

public partial class AnnouncementItemCacheEventConsumer : CacheEventConsumer<AnnouncementItem>
{
    protected override async Task ClearCacheAsync(AnnouncementItem entity)
    {
        await RemoveByPrefixAsync(NopStationEntityCacheDefaults<AnnouncementItem>.Prefix);
        await RemoveByPrefixAsync(AnnouncementCacheDefaults.AnnouncementItemPrefix);
    }
}