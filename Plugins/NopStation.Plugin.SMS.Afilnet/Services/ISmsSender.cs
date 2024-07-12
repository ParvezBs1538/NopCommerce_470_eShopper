using System.Threading.Tasks;
using NopStation.Plugin.SMS.Afilnet.Domains;

namespace NopStation.Plugin.SMS.Afilnet.Services
{
    public interface ISmsSender
    {
        Task SendNotificationAsync(QueuedSms queuedSms);

        Task SendNotificationAsync(string phoneNumber, string body);
    }
}