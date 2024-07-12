using System;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public class QueuedWebApiNotificationSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IPushNotificationSender _pushNotificationSender;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;

        public QueuedWebApiNotificationSendTask(ILogger logger,
            IPushNotificationSender pushNotificationSender,
            IQueuedPushNotificationService queuedPushNotificationService)
        {
            _logger = logger;
            _pushNotificationSender = pushNotificationSender;
            _queuedPushNotificationService = queuedPushNotificationService;
        }

        public async Task ExecuteAsync()
        {
            var queuedPushNotifications = await _queuedPushNotificationService.GetAllQueuedPushNotificationsAsync(false, true);

            for (int i = 0; i < queuedPushNotifications.Count; i++)
            {
                var queuedPushNotification = queuedPushNotifications[i];
                try
                {
                    if (await _pushNotificationSender.SendNotification(queuedPushNotification))
                    {
                        queuedPushNotification.SentOnUtc = DateTime.UtcNow;
                        queuedPushNotification.ErrorLog = "";
                    }
                    else
                    {
                        queuedPushNotification.ErrorLog = $"{queuedPushNotification.ErrorLog}{queuedPushNotification.SentTries + 1}.{Environment.NewLine}";
                        queuedPushNotification.SentTries = queuedPushNotification.SentTries + 1;
                        await _logger.ErrorAsync($"Failed to send app push notification (Id = {queuedPushNotification.Id})");
                    }
                    await _queuedPushNotificationService.UpdateQueuedPushNotificationAsync(queuedPushNotification);
                }
                catch (Exception ex)
                {
                    queuedPushNotification.ErrorLog = $"{queuedPushNotification.ErrorLog}{queuedPushNotification.SentTries + 1}. {ex.Message}{Environment.NewLine}";
                    queuedPushNotification.SentTries = queuedPushNotification.SentTries + 1;
                    await _queuedPushNotificationService.UpdateQueuedPushNotificationAsync(queuedPushNotification);

                    await _logger.ErrorAsync($"Failed to send app push notification (Id = {queuedPushNotification.Id}): {ex.Message}", ex);

                    continue;
                }
            }
        }
    }
}
