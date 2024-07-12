using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public interface IPushNotificationSender
    {
        Task<bool> SendNotification(WebApiQueuedNotification queuedPushNotification);

        Task<bool> SendNotification(DeviceType deviceType, string title, string body, string subscriptionId,
            int actionTypeId = 0, string actionValue = "", string imageUrl = "");
    }
}