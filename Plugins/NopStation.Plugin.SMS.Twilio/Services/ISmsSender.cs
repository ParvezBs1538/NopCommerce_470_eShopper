using System.Threading.Tasks;
using NopStation.Plugin.SMS.Twilio.Domains;

namespace NopStation.Plugin.SMS.Twilio.Services
{
    public interface ISmsSender
    {
        Task SendNotificationAsync(QueuedSms queuedSms);

        Task SendNotificationAsync(string phoneNumber, string body);
    }
}