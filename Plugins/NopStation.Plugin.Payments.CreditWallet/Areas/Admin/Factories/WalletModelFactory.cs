using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Models;
using NopStation.Plugin.Payments.CreditWallet.Domain;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Areas.Admin.Factories
{
    public class WalletModelFactory : IWalletModelFactory
    {
        private readonly ICustomerService _customerService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IWalletService _walletService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        public WalletModelFactory(ICustomerService customerService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IWalletService walletService,
            CustomerSettings customerSettings,
            ICurrencyService currencyService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _customerService = customerService;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _walletService = walletService;
            _customerSettings = customerSettings;
            _currencyService = currencyService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        public async Task<WalletModel> PrepareWalletModelAsync(WalletModel model, Wallet wallet, Customer customer,
            bool excludeProperties = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (wallet != null)
            {
                if (model == null)
                {
                    model = wallet.ToModel<WalletModel>();
                    model.AvailableCredit = wallet.AvailableCredit;

                    var currency = await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId);
                    model.CurrencyCode = currency.CurrencyCode;
                }
            }

            model.WalletCustomerId = customer.Id;
            model.WalletCustomerEmail = customer.Email;
            model.WalletCustomerName = await _customerService.GetCustomerFullNameAsync(customer);

            if (!excludeProperties)
            {
                await _baseAdminModelFactory.PrepareCurrenciesAsync(model.AvailableCurrencies, false);
            }

            return model;
        }

        public async Task<WalletListModel> PrepareWalletListModelAsync(WalletSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var wallets = await _walletService.GetAllWalletsAsync(
                crids: searchModel.SelectedCustomerRoleIds.ToArray(),
                email: searchModel.SearchEmail,
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new WalletListModel().PrepareToGridAsync(searchModel, wallets, () =>
            {
                return wallets.SelectAwait(async wallet =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(wallet.WalletCustomerId);
                    return await PrepareWalletModelAsync(null, wallet, customer, true);
                });
            });

            return model;
        }

        public async Task<WalletSearchModel> PrepareWalletSearchModelAsync(WalletSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.FirstNameEnabled = _customerSettings.FirstNameEnabled;
            searchModel.LastNameEnabled = _customerSettings.LastNameEnabled;

            var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
            if (registeredRole != null)
                searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

            //prepare available customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }
    }
}
