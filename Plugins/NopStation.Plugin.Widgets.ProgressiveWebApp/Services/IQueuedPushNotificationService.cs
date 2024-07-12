using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IQueuedPushNotificationService
    {
        Task DeleteQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification);

        Task InsertQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification);

        Task UpdateQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification);

        Task<QueuedPushNotification> GetQueuedPushNotificationByIdAsync(int queuedPushNotificationId);

        Task<IPagedList<QueuedPushNotification>> GetAllQueuedPushNotificationsAsync(bool? sentItems = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}