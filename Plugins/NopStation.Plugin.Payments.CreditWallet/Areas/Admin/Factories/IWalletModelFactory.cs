using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public interface IWalletModelFactory
    {
        Task<WalletSearchModel> PrepareWalletSearchModelAsync(WalletSearchModel searchModel);

        Task<WalletListModel> PrepareWalletListModelAsync(WalletSearchModel searchModel);

        Task<WalletModel> PrepareWalletModelAsync(WalletModel model, Wallet wallet,
            Customer customer, bool excludeProperties = false);
    }
}
