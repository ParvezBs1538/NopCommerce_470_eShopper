using NopStation.Plugin.SMS.Messente.Services;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Messente
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly MessenteSmsSettings _messenteSmsSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender afilnetsmsSender,
            IQueuedSmsService queuedSmsService,
            MessenteSmsSettings messenteSmsSettings)
        {
            _logger = logger;
            _smsSender = afilnetsmsSender;
            _queuedSmsService = queuedSmsService;
            _messenteSmsSettings = messenteSmsSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_messenteSmsSettings.EnablePlugin)
                return;

            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(true, 2);

            foreach (var queuedSms in queuedSmss)
            {
                try
                {
                    _smsSender.SendNotification(queuedSms);
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
