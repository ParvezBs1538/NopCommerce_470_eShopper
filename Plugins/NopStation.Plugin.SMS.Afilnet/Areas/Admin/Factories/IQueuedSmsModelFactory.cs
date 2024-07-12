using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.SMS.Afilnet.Areas.Admin.Models;
using NopStation.Plugin.SMS.Afilnet.Domains;

namespace NopStation.Plugin.SMS.Afilnet.Areas.Admin.Factories
{
    public interface IQueuedSmsModelFactory
    {
        QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel);

        Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, 
            QueuedSms queuedSms, bool excludeProperties = false);
    }
}