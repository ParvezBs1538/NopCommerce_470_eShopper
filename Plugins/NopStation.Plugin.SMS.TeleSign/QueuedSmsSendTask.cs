using NopStation.Plugin.SMS.TeleSign.Services;
using Nop.Services.Logging;
using System;
using Nop.Services.ScheduleTasks;

namespace NopStation.Plugin.SMS.TeleSign
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly TeleSignSettings _teleSignSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender teleSignsmsSender,
            IQueuedSmsService queuedSmsService,
            TeleSignSettings teleSignSettings)
        {
            _logger = logger;
            _smsSender = teleSignsmsSender;
            _queuedSmsService = queuedSmsService;
            _teleSignSettings = teleSignSettings;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            if (!_teleSignSettings.EnablePlugin)
                return;

            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(true, 2);

            foreach (var queuedSms in queuedSmss)
            {
                try
                {
                    var response = _smsSender.SendNotification(queuedSms);

                    if (response.Status.Code != 1)
                    {
                        queuedSms.SentTries++;
                        queuedSms.Error += $"{queuedSms.SentTries}. <p>{response.Status.Description}</p></br>";
                    }
                    else
                    {
                        queuedSms.SentOnUtc = DateTime.UtcNow;
                    }

                    await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message, ex);
                    queuedSms.SentTries++;
                    queuedSms.Error += $"{queuedSms.SentTries}. {ex.Message}</br>";
                    await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                }
            }
        }
    }
}
