using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.MessageBird.Areas.Admin.Models;
using NopStation.Plugin.SMS.MessageBird.Domains;

namespace NopStation.Plugin.SMS.MessageBird.Areas.Admin.Factories
{
    public interface IQueuedSmsModelFactory
    {
        QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, 
            QueuedSms queuedSms, bool excludeProperties = false);
    }
}