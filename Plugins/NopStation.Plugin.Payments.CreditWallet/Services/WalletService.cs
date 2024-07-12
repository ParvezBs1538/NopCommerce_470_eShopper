using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Directory;
using NopStation.Plugin.Payments.CreditWallet.Domain;

namespace NopStation.Plugin.Payments.CreditWallet.Services
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _roleMappingRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly ICurrencyService _currencyService;

        public WalletService(IRepository<Wallet> walletRepository,
            IRepository<CustomerCustomerRoleMapping> roleMappingRepository,
            IRepository<GenericAttribute> gaRepository,
            IRepository<Customer> customerRepository,
            ICurrencyService currencyService)
        {
            _walletRepository = walletRepository;
            _roleMappingRepository = roleMappingRepository;
            _gaRepository = gaRepository;
            _customerRepository = customerRepository;
            _currencyService = currencyService;
        }

        public async Task InsertWalletAsync(Wallet model)
        {
            await _walletRepository.InsertAsync(model);
        }

        public async Task UpdateWalletAsync(Wallet model)
        {
            await _walletRepository.UpdateAsync(model);
        }

        public async Task DeleteWalletAsync(Wallet model)
        {
            await _walletRepository.DeleteAsync(model);
        }

        public async Task<Wallet> GetWalletByCustomerIdAsync(int customerId)
        {
            var query = from c in _walletRepository.Table
                        where c.WalletCustomerId == customerId
                        select c;
            var customerCreditDetails = await query.FirstOrDefaultAsync();

            return customerCreditDetails;
        }

        public async Task<IPagedList<Wallet>> GetAllWalletsAsync(int[] crids = null, string email = null,
            string firstName = null, string lastName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var wallets = await _walletRepository.GetAllPagedAsync(query =>
            {
                if (crids != null && crids.Length > 0)
                {
                    query = query.Join(_roleMappingRepository.Table, x => x.WalletCustomerId, y => y.CustomerId,
                            (x, y) => new { Wallet = x, Mapping = y })
                        .Where(z => crids.Contains(z.Mapping.CustomerRoleId))
                        .Select(z => z.Wallet)
                        .Distinct();
                }
                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Join(_customerRepository.Table, x => x.WalletCustomerId, y => y.Id,
                            (x, y) => new { Wallet = x, Customer = y })
                        .Where(z => z.Customer.Email.Contains(email))
                        .Select(z => z.Wallet);
                }
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    query = query
                        //.Join(_gaRepository.Table, x => x.WalletCustomerId, y => y.EntityId,
                        //    (x, y) => new { Wallet = x, Attribute = y })
                        //.Join(_customerRepository.Table, x=>x.WalletCustomerId, y=>y.Id)
                        .Join(_customerRepository.Table, x => x.WalletCustomerId, y => y.Id,
                            (x, y) => new { Wallet = x, Customer = y })
                        .Where(z => z.Customer.FirstName.Contains(firstName))
                        .Select(z => z.Wallet);
                }
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    query = query
                        //.Join(_gaRepository.Table, x => x.WalletCustomerId, y => y.EntityId,
                        //    (x, y) => new { Wallet = x, Attribute = y })
                        .Join(_customerRepository.Table, x => x.WalletCustomerId, y => y.Id,
                            (x, y) => new { Wallet = x, Customer = y })
                        .Where(z => z.Customer.LastName.Contains(lastName))
                        .Select(z => z.Wallet);
                }

                return query;
            }, pageIndex, pageSize);

            return wallets;
        }

        public async Task<bool> HasSufficientBalance(Wallet wallet, decimal orderTotal)
        {
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet));

            var currency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
            var convertedOrderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal, currency);
            return convertedOrderTotal <= wallet.AvailableCredit;
        }
    }
}
