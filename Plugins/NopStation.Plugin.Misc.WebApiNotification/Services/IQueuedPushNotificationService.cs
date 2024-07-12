using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public interface IQueuedPushNotificationService
    {
        Task DeleteQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification);

        Task DeleteSentQueuedPushNotificationAsync();

        Task InsertQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification);

        Task UpdateQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification);

        Task<WebApiQueuedNotification> GetQueuedPushNotificationByIdAsync(int queuedPushNotificationId);

        Task<IPagedList<WebApiQueuedNotification>> GetAllQueuedPushNotificationsAsync(bool? sentStatus = null,
            bool enableDateConsideration = false, DateTime? sentFromUtc = null, DateTime? sentToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}