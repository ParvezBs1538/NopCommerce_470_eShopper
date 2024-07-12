using NopStation.Plugin.SMS.BulkSms.Services;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using System;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.BulkSms
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly BulkSmsSettings _bulkSmsSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender afilnetsmsSender,
            IQueuedSmsService queuedSmsService,
            BulkSmsSettings bulkSmsSettings)
        {
            _logger = logger;
            _smsSender = afilnetsmsSender;
            _queuedSmsService = queuedSmsService;
            _bulkSmsSettings = bulkSmsSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_bulkSmsSettings.EnablePlugin)
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
