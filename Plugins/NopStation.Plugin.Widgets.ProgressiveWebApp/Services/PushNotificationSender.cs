using System;
using Newtonsoft.Json;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services.Models;
using WebPush;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class PushNotificationSender : IPushNotificationSender
    {
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        public PushNotificationSender(ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public void SendNotification(WebAppDevice device, QueuedPushNotification queuedPushNotification)
        {
            var body = queuedPushNotification.Body.Replace(Environment.NewLine, "\n");
            var direction = queuedPushNotification.Rtl ? "rtl" : "ltr";
            var iconUrl = queuedPushNotification.IconUrl;
            var imageUrl = queuedPushNotification.ImageUrl;
            var title = queuedPushNotification.Title;
            var url = queuedPushNotification.Url;

            SendNotification(device, title, body, direction, iconUrl, imageUrl, url);
        }

        public void SendNotification(WebAppDevice device, string title, string body, string direction,
            string iconUrl = null, string imageUrl = null, string url = null)
        {
            var model = new PushNotificationModel()
            {
                Body = body,
                Direction = direction,
                IconUrl = iconUrl,
                ImageUrl = imageUrl,
                SoundFileUrl = _progressiveWebAppSettings.SoundFileUrl,
                Vibration = _progressiveWebAppSettings.Vibration,
                Data = new PushNotificationModel.DataModel()
                {
                    Title = title,
                    Url = url
                }
            };

            var jsonString = JsonConvert.SerializeObject(model);
            var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
            var vapidDetails = new VapidDetails($"mailto:{_progressiveWebAppSettings.VapidSubjectEmail}", device.VapidPublicKey, device.VapidPrivateKey);

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, jsonString, vapidDetails);
        }
    }
}
