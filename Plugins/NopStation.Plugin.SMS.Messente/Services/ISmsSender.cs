using System.Collections.Generic;
using NopStation.Plugin.SMS.Messente.Domains;

namespace NopStation.Plugin.SMS.Messente.Services
{
    public interface ISmsSender
    {
        void SendNotification(QueuedSms queuedSms);

        void SendNotification(string phoneNumber, string body);
    }
}