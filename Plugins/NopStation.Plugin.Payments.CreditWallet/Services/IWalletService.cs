using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public interface IWalletService
    {
        Task InsertWalletAsync(Wallet wallet);

        Task UpdateWalletAsync(Wallet wallet);

        Task DeleteWalletAsync(Wallet wallet);

        Task<Wallet> GetWalletByCustomerIdAsync(int nopCustomerId);

        Task<IPagedList<Wallet>> GetAllWalletsAsync(int[] crids = null, string email = null,
            string firstName = null, string lastName = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<bool> HasSufficientBalance(Wallet wallet, decimal orderTotal);
    }
}
