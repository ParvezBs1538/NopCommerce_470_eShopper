﻿using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.Vonage.Domains;

namespace NopStation.Plugin.SMS.Vonage.Services
{
    public interface IQueuedSmsService
    {
        Task DeleteQueuedSmsAsync(QueuedSms queuedSms);

        Task InsertQueuedSmsAsync(QueuedSms queuedSms);

        Task UpdateQueuedSmsAsync(QueuedSms queuedSms);

        Task<QueuedSms> GetQueuedSmsByIdAsync(int queuedSmsId);

        Task<IPagedList<QueuedSms>> GetAllQueuedSmsAsync(bool loadOnlyItemsToBeSent,
             int maxSentTries = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}