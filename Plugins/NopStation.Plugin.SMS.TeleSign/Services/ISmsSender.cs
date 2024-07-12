using NopStation.Plugin.SMS.TeleSign.Domains;
using NopStation.Plugin.SMS.TeleSign.Services.Responses;

namespace NopStation.Plugin.SMS.TeleSign.Services
{
    public interface ISmsSender
    {
        TelesignResponse SendNotification(QueuedSms queuedSms);

        TelesignResponse SendNotification(string phoneNumber, string body, string customerId = null);
    }
}