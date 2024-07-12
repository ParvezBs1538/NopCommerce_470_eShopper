using Newtonsoft.Json;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using WebPush;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class WebPushService : IWebPushService
    {
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly IWebAppDeviceService _webAppDeviceService;

        public WebPushService(ProgressiveWebAppSettings progressiveWebAppSettings,
            IWebAppDeviceService webAppDeviceService)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _webAppDeviceService = webAppDeviceService;
        }

        public bool ValidDevice(string endpoint, string p256dh, string auth)
        {
            try
            {
                SendNotification(endpoint, p256dh, auth, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SendNotification(WebAppDevice device, object data)
        {
            SendNotification(device.PushEndpoint, device.PushP256DH, device.PushAuth, data);
        }

        public void SendNotification(string endpoint, string p256dh, string auth, object data)
        {
            var payload = data != null ? JsonConvert.SerializeObject(data) : null;
            var pushSubscription = new PushSubscription(endpoint, p256dh, auth);
            var vapidDetails = new VapidDetails("mailto:example@example.com",
                _progressiveWebAppSettings.VapidPublicKey, _progressiveWebAppSettings.VapidPrivateKey);

            var webPushClient = new WebPushClient();
            webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
        }
    }
}
