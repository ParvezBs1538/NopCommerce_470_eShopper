using NopStation.Plugin.SMS.MessageBird.Domains;
using Nop.Services.Logging;
using MessageBird;
using MessageBird.Objects;
using System;

namespace NopStation.Plugin.SMS.MessageBird.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly MessageBirdSettings _messageBirdSettings;
        private readonly ILogger _logger;

        public SmsSender(MessageBirdSettings messageBirdSettings,
            ILogger logger)
        {
            _messageBirdSettings = messageBirdSettings;
            _logger = logger;
        }

        public Message SendNotification(QueuedSms queuedSms)
        {
            return SendNotification(queuedSms.PhoneNumber, queuedSms.Body);
        }

        public Message SendNotification(string phoneNumber, string body)
        {
            if (phoneNumber.StartsWith("+"))
                phoneNumber = phoneNumber.Trim('+');

            var longNumber = Convert.ToInt64(phoneNumber);
            var client = Client.CreateDefault(_messageBirdSettings.AccessKey);
            var message = client.SendMessage(_messageBirdSettings.Originator, body, new[] { longNumber });

            return message;
        }
    }
}
