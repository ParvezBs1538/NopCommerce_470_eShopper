using NopStation.Plugin.SMS.Vonage.Services;
using Nop.Services.Logging;
using System;
using System.Linq;
using Nop.Services.ScheduleTasks;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Vonage
{
    public class QueuedSmsSendTask : IScheduleTask
    {
        private readonly ILogger _logger;
        private readonly ISmsSender _smsSender;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly VonageSettings _vonageSettings;

        public QueuedSmsSendTask(ILogger logger,
            ISmsSender vonagesmsSender,
            IQueuedSmsService queuedSmsService,
            VonageSettings vonageSettings)
        {
            _logger = logger;
            _smsSender = vonagesmsSender;
            _queuedSmsService = queuedSmsService;
            _vonageSettings = vonageSettings;
        }

        public async Task ExecuteAsync()
        {
            if (!_vonageSettings.EnablePlugin)
                return;

            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(true, 2);

            foreach (var queuedSms in queuedSmss)
            {
                try
                {
                    var response = await _smsSender.SendNotificationAsync(queuedSms);

                    if (response.Messages.Any(x => x.Status != "0") && response.Messages.Any(x => x.Status == "0"))
                    {
                        if (_vonageSettings.EnableLog)
                            await _logger.ErrorAsync("Message partially sent!");

                        var error = "Message partially sent!</br>";
                        var i = 1;
                        foreach (var message in response.Messages.Where(x => x.Status != "0"))
                            error += $"{i++}. {message.ErrorText}</br>";

                        queuedSms.Error += $"{queuedSms.SentTries}. <p>{error}</p></br>";
                        queuedSms.MessageCount = response.MessageCount;
                        queuedSms.RemainingBalance = response.Messages.Min(x => Convert.ToInt32(x.RemainingBalance)).ToString();
                        queuedSms.MessagePrice = response.Messages.Sum(x => Convert.ToDecimal(x.MessagePrice)).ToString();
                        queuedSms.Network = response.Messages.Where(x => x.Status == "0").FirstOrDefault()?.Network;
                        queuedSms.AccountRef = response.Messages.Where(x => x.Status == "0").FirstOrDefault()?.AccountRef;
                        queuedSms.MessageId = response.Messages.Where(x => x.Status == "0").FirstOrDefault()?.MessageId;
                        queuedSms.SentOnUtc = DateTime.UtcNow;
                    }
                    else if (response.Messages.All(x => x.Status != "0"))
                    {
                        var error = "";
                        var i = 1;
                        foreach (var message in response.Messages.Where(x => x.Status != "0"))
                            error += $"{i++}. {message.ErrorText}</br>";

                        queuedSms.MessageCount = response.MessageCount;
                        queuedSms.SentTries++;
                        queuedSms.Error += $"{queuedSms.SentTries}. <p>{error}</p></br>";
                    }
                    else
                    {
                        queuedSms.MessageCount = response.MessageCount;
                        queuedSms.RemainingBalance = response.Messages.Min(x => Convert.ToInt32(x.RemainingBalance)).ToString();
                        queuedSms.MessagePrice = response.Messages.Sum(x => Convert.ToDecimal(x.MessagePrice)).ToString();
                        queuedSms.Network = response.Messages.FirstOrDefault()?.Network;
                        queuedSms.AccountRef = response.Messages.FirstOrDefault()?.AccountRef;
                        queuedSms.MessageId = response.Messages.FirstOrDefault()?.MessageId;
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
