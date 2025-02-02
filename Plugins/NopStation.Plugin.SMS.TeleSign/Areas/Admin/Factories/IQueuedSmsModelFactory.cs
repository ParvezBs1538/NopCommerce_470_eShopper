﻿using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models;
using NopStation.Plugin.SMS.TeleSign.Domains;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Factories
{
    public interface IQueuedSmsModelFactory
    {
        QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, 
            QueuedSms queuedSms, bool excludeProperties = false);
    }
}