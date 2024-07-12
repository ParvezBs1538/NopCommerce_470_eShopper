using com.Messente.Api.Api;
using com.Messente.Api.Client;
using com.Messente.Api.Model;
using NopStation.Plugin.SMS.Messente.Domains;
using System.Collections.Generic;

namespace NopStation.Plugin.SMS.Messente.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly MessenteSmsSettings _messenteSmsSettings;

        public SmsSender(MessenteSmsSettings messenteSmsSettings)
        {
            _messenteSmsSettings = messenteSmsSettings;
        }

        public void SendNotification(QueuedSms queuedSms)
        {
            SendNotification(queuedSms.PhoneNumber, queuedSms.Body);
        }

        public void SendNotification(string phoneNumber, string body)
        {
            var conf = new Configuration();
            conf.Username = _messenteSmsSettings.Username;
            conf.Password = _messenteSmsSettings.Password;
            var apiInstance = new OmnimessageApi(conf);

            var sms = new com.Messente.Api.Model.SMS(sender: _messenteSmsSettings.SenderName, text: body);
            var messages = new List<object>();
            messages.Add(sms);

            var omnimessage = new Omnimessage(
                to: phoneNumber,
                messages: messages
            );

            apiInstance.SendOmnimessage(omnimessage);
        }
    }
}
