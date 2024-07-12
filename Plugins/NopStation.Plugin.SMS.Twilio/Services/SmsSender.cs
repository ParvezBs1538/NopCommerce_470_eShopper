using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NopStation.Plugin.SMS.Twilio.Domains;
using Nop.Services.Logging;
using Twilio.Rest.Api.V2010.Account;
using Twilio;
using Twilio.Types;

namespace NopStation.Plugin.SMS.Twilio.Services
{
    public class SmsSender : ISmsSender
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly ILogger _logger;

        public SmsSender(TwilioSettings twilioSettings,
            ILogger logger)
        {
            _twilioSettings = twilioSettings;
            _logger = logger;
        }

        public async Task SendNotificationAsync(QueuedSms queuedSms)
        {
            await SendNotificationAsync(queuedSms.PhoneNumber, queuedSms.Body);
        }

        public async Task SendNotificationAsync(string phoneNumber, string body)
        {
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);

            var message = MessageResource.Create(
                body: body,
                from: new PhoneNumber(_twilioSettings.PhoneNumber),
                to: new PhoneNumber(phoneNumber)
            );

            if (message.ErrorCode.HasValue)
            {
                await _logger.ErrorAsync(JsonConvert.SerializeObject(message));
                throw new Exception(message.ErrorMessage);
            }

            if (_twilioSettings.EnableLog)
                await _logger.InformationAsync(message.Sid);
        }
    }
}
