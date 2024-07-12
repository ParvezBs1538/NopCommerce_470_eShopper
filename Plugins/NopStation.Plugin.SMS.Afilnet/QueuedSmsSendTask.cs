using NopStation.Plugin.SMS.Afilnet.Services;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Afilnet
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly AfilnetSettings _afilnetSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender afilnetsmsSender,
            IQueuedSmsService queuedSmsService,
            AfilnetSettings afilnetSettings)
        {
            _logger = logger;
            _smsSender = afilnetsmsSender;
            _queuedSmsService = queuedSmsService;
            _afilnetSettings = afilnetSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_afilnetSettings.EnablePlugin)
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
