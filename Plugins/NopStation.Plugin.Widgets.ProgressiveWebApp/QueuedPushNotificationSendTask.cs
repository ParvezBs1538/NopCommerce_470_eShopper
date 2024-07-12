using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp
{
    public class QueuedPushNotificationSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly IPushNotificationSender _pushNotificationSender;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IWebAppDeviceService _webAppDeviceService;

        public QueuedPushNotificationSendTask(ILogger logger,
            IPushNotificationSender pushNotificationSender,
            IQueuedPushNotificationService queuedPushNotificationService,
            IWebAppDeviceService webAppDeviceService)
        {
            _logger = logger;
            _pushNotificationSender = pushNotificationSender;
            _queuedPushNotificationService = queuedPushNotificationService;
            _webAppDeviceService = webAppDeviceService;
        }

        public async Task ExecuteAsync()
        {
            var queuedPushNotifications = await _queuedPushNotificationService.GetAllQueuedPushNotificationsAsync(false);

            foreach (var queuedPushNotification in queuedPushNotifications)
            {
                var devices = await (await _webAppDeviceService.GetWebAppDevicesAsync(queuedPushNotification.CustomerId,
                    queuedPushNotification.StoreId)).ToListAsync();

                var deleteDevices = new List<WebAppDevice>();

                foreach (var device in devices)
                {
                    try
                    {
                        _pushNotificationSender.SendNotification(device, queuedPushNotification);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Subscription no longer valid" ||
                            ex.Message == "Received unexpected response code: 403")
                            deleteDevices.Add(device);

                        await _logger.ErrorAsync($"PushP256DH: {device.PushP256DH}{Environment.NewLine}" +
                            $"PushAuth : {device.PushAuth}{Environment.NewLine}CustomerId: {device.CustomerId}" +
                            $"{Environment.NewLine}" + ex.Message, ex);

                        continue;
                    }
                }

                if (deleteDevices.Any())
                    await _webAppDeviceService.DeleteWebAppDeviceAsync(deleteDevices);
            }

            for (var i = 0; i < queuedPushNotifications.Count; i++)
            {
                var queuedPushNotification = queuedPushNotifications[i];
                queuedPushNotification.SentOnUtc = DateTime.UtcNow;
                await _queuedPushNotificationService.UpdateQueuedPushNotificationAsync(queuedPushNotification);
            }
        }
    }
}
