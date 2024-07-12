using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services.Cache
{
    public partial class QueuedPushNotificationCacheEventConsumer : CacheEventConsumer<WebApiQueuedNotification>
    {
        protected override async Task ClearCacheAsync(WebApiQueuedNotification entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<WebApiQueuedNotification>.Prefix);
        }
    }
}