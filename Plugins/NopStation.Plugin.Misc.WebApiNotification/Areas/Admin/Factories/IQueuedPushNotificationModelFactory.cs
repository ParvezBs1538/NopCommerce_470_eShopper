using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public interface IQueuedPushNotificationModelFactory
    {
        WebApiQueuedNotificationSearchModel PrepareQueuedPushNotificationSearchModel(WebApiQueuedNotificationSearchModel searchModel);

        Task<WebApiQueuedNotificationListModel> PrepareQueuedPushNotificationListModelAsync(WebApiQueuedNotificationSearchModel searchModel);

        Task<WebApiQueuedNotificationModel> PrepareQueuedPushNotificationModelAsync(WebApiQueuedNotificationModel model,
            WebApiQueuedNotification queuedPushNotification, bool excludeProperties = false);
    }
}