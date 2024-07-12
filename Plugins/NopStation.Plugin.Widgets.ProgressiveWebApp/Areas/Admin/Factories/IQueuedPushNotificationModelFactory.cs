using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public interface IQueuedPushNotificationModelFactory
    {
        QueuedPushNotificationSearchModel PrepareQueuedPushNotificationSearchModel(QueuedPushNotificationSearchModel searchModel);

        Task<QueuedPushNotificationListModel> PrepareQueuedPushNotificationListModelAsync(QueuedPushNotificationSearchModel searchModel);

        Task<QueuedPushNotificationModel> PrepareQueuedPushNotificationModelAsync(QueuedPushNotificationModel model,
            QueuedPushNotification queuedPushNotification, bool excludeProperties = false);
    }
}