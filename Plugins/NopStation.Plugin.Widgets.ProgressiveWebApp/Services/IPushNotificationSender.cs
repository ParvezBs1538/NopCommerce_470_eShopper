using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IPushNotificationSender
    {
        void SendNotification(WebAppDevice device, QueuedPushNotification queuedPushNotification);

        void SendNotification(WebAppDevice device, string title, string body, string direction,
            string iconUrl = null, string imageUrl = null, string url = null);
    }
}