using System.Threading.Tasks;
using NopStation.Plugin.SMS.SmsTo.Domains;

namespace NopStation.Plugin.SMS.SmsTo.Services
{
    public interface ISmsSender
    {
        Task SendNotificationAsync(QueuedSms queuedSms);

        Task SendNotificationAsync(string phoneNumber, string body, string customerId = null);
    }
}