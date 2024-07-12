using NopStation.Plugin.SMS.TeleSign.Domains;
using Telesign;
using Newtonsoft.Json;
using NopStation.Plugin.SMS.TeleSign.Services.Responses;
using Newtonsoft.Json.Linq;

namespace NopStation.Plugin.SMS.TeleSign.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly TeleSignSettings _teleSignSettings;

        public SmsSender(TeleSignSettings teleSignSettings)
        {
            _teleSignSettings = teleSignSettings;
        }

        public TelesignResponse SendNotification(QueuedSms queuedSms)
        {
            return SendNotification(queuedSms.PhoneNumber, queuedSms.Body, queuedSms.CustomerId?.ToString());
        }

        public TelesignResponse SendNotification(string phoneNumber, string body, string customerId = null)
        {
            var messagingClient = new MessagingClient(customerId, _teleSignSettings.ApiKey);
            var telesignResponse = messagingClient.Message(phoneNumber, body, "ARN");

            var serializer = new JsonSerializer();
            var response = (TelesignResponse)serializer.Deserialize(new JTokenReader(telesignResponse.Json), typeof(TelesignResponse));

            return response;
        }
    }
}
