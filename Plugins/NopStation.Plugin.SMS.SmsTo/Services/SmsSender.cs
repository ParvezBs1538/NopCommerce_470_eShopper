using System.Threading.Tasks;
using NopStation.Plugin.SMS.SmsTo.Domains;
using Nop.Services.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace NopStation.Plugin.SMS.SmsTo.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly SmsToSettings _smsToSettings;
        private readonly ILogger _logger;

        public SmsSender(SmsToSettings smsToSettings,
            ILogger logger)
        {
            _smsToSettings = smsToSettings;
            _logger = logger;
        }

        public async Task SendNotificationAsync(QueuedSms queuedSms)
        {
            await SendNotificationAsync(queuedSms.PhoneNumber, queuedSms.Body, queuedSms.CustomerId?.ToString());
        }

        public async Task SendNotificationAsync(string phoneNumber, string body, string customerId = null)
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.sms.to/sms/send");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_smsToSettings.ApiKey}");

            var data = new
            {
                message = body,
                to = phoneNumber,
                sender_id = _smsToSettings.SenderId
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            await httpClient.SendAsync(request);
        }
    }
}
