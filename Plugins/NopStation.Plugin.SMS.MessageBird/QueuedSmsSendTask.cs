using Newtonsoft.Json;
using NopStation.Plugin.SMS.MessageBird.Services;
using Nop.Services.Logging;
using System;
using System.Linq;
using Nop.Services.ScheduleTasks;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.MessageBird
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly MessageBirdSettings _messageBirdSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender messageBirdsmsSender,
            IQueuedSmsService queuedSmsService,
            MessageBirdSettings messageBirdSettings)
        {
            _logger = logger;
            _smsSender = messageBirdsmsSender;
            _queuedSmsService = queuedSmsService;
            _messageBirdSettings = messageBirdSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_messageBirdSettings.EnablePlugin)
                return;

            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(true, 2);

            foreach (var queuedSms in queuedSmss)
            {
                try
                {
                    var message = _smsSender.SendNotification(queuedSms);
                    if (message.Recipients.Items.Any())
                    {
                        queuedSms.SentOnUtc = DateTime.UtcNow;
                        await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                    }
                    else
                    {
                        await _logger.ErrorAsync(JsonConvert.SerializeObject(message));
                        queuedSms.SentTries++;
                        queuedSms.Error += $"{queuedSms.SentTries}. Failed to send sms<br>";
                        await _queuedSmsService.UpdateQueuedSmsAsync(queuedSms);
                    }
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
