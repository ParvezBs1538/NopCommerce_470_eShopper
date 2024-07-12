using System.Threading.Tasks;
using NopStation.Plugin.SMS.Vonage.Domains;
using Vonage.Messaging;

namespace NopStation.Plugin.SMS.Vonage.Services
{
    public interface ISmsSender
    {
        Task<SendSmsResponse> SendNotificationAsync(QueuedSms queuedSms);

        Task<SendSmsResponse> SendNotificationAsync(string phoneNumber, string body, string customerId = null);
    }
}