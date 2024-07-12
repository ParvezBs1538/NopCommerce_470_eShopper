using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.Afilnet.Domains;

namespace NopStation.Plugin.SMS.Afilnet.Services
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