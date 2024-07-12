using System.Collections.Generic;
using NopStation.Plugin.SMS.BulkSms.Domains;

namespace NopStation.Plugin.SMS.BulkSms.Services
{
    public interface ISmsSender
    {
        void SendNotification(QueuedSms queuedSms);

        void SendNotification(IList<SmsParameter> smsParameters);
    }
}