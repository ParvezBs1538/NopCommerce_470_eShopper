using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IWebPushService
    {
        bool ValidDevice(string endpoint, string p256dh, string auth);

        void SendNotification(WebAppDevice device, object data);

        void SendNotification(string endpoint, string p256dh, string auth, object data);
    }
}