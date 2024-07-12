using NopStation.Plugin.SMS.Twilio.Domains;
using NopStation.Plugin.SMS.Twilio.Services;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Twilio
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly TwilioSettings _twilioSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender twiliosmsSender,
            IQueuedSmsService queuedSmsService,
            TwilioSettings twilioSettings)
        {
            _logger = logger;
            _smsSender = twiliosmsSender;
            _queuedSmsService = queuedSmsService;
            _twilioSettings = twilioSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_twilioSettings.EnablePlugin)
                return;

            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(true, 2);

            foreach (var queuedSms in queuedSmss)
            {
                try
                {
                    await _smsSender.SendNotificationAsync(queuedSms);
                    queuedSms.SentOnUtc = DateTime.UtcNow;
                    await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex);
                    queuedSms.SentTries++;
                    queuedSms.Error += $"{queuedSms.SentTries}. {ex.Message}<br>";
                    await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                }
            }
        }
    }
}
