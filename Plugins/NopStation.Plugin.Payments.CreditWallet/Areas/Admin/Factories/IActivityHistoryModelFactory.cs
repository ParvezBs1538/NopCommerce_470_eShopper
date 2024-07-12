using System.Threading.Tasks;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public interface IActivityHistoryModelFactory
    {
        Task<ActivityHistorySearchModel> PrepareActivityHistorySearchModelAsync(ActivityHistorySearchModel searchModel, Wallet wallet = null);

        Task<ActivityHistoryListModel> PrepareActivityHistoryListModelAsync(ActivityHistorySearchModel searchModel);

        Task<ActivityHistoryModel> PrepareActivityHistoryModelAsync(ActivityHistoryModel model, ActivityHistory activityHistory,
            Wallet wallet, bool excludeProperties = false);
    }
}
