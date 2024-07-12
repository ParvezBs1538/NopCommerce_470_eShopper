using NopStation.Plugin.SMS.SmsTo.Services;
using Nop.Services.Logging;
using System;
using Nop.Services.ScheduleTasks;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.SmsTo
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly SmsToSettings _smsToSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender smsTosmsSender,
            IQueuedSmsService queuedSmsService,
            SmsToSettings smsToSettings)
        {
            _logger = logger;
            _smsSender = smsTosmsSender;
            _queuedSmsService = queuedSmsService;
            _smsToSettings = smsToSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_smsToSettings.EnablePlugin)
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
