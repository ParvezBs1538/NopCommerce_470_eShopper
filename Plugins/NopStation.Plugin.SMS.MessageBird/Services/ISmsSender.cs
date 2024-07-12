using MessageBird.Objects;
using NopStation.Plugin.SMS.MessageBird.Domains;

namespace NopStation.Plugin.SMS.MessageBird.Services
{
    public interface ISmsSender
    {
        Message SendNotification(QueuedSms queuedSms);

        Message SendNotification(string phoneNumber, string body);
    }
}