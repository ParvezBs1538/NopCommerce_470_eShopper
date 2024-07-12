using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NopStation.Plugin.SMS.Vonage.Domains;
using Nop.Services.Logging;
using Vonage;
using Vonage.Messaging;
using Vonage.Request;

namespace NopStation.Plugin.SMS.Vonage.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly VonageSettings _vonageSettings;
        private readonly ILogger _logger;

        public SmsSender(VonageSettings vonageSettings,
            ILogger logger)
        {
            _vonageSettings = vonageSettings;
            _logger = logger;
        }

        public async Task<SendSmsResponse> SendNotificationAsync(QueuedSms queuedSms)
        {
            return await SendNotificationAsync(queuedSms.PhoneNumber, queuedSms.Body, queuedSms.CustomerId?.ToString());
        }

        public async Task<SendSmsResponse> SendNotificationAsync(string phoneNumber, string body, string customerId = null)
        {
            var credentials = Credentials.FromApiKeyAndSecret(
                    _vonageSettings.ApiKey,
                    _vonageSettings.ApiSecret
                );

            var vonageClient = new VonageClient(credentials);

            var response = await vonageClient.SmsClient.SendAnSmsAsync(new SendSmsRequest()
            {
                To = phoneNumber,
                From = _vonageSettings.From,
                Text = body,
                AccountRef = phoneNumber,
                ClientRef = customerId
            });

            if (_vonageSettings.EnableLog)
                await _logger.InformationAsync(JsonConvert.SerializeObject(response));

            return response;
        }
    }
}
