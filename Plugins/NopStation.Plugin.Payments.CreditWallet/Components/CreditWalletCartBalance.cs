using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Components
{
    public class CreditWalletCartBalanceViewComponent : NopStationViewComponent
    {
        private readonly CreditWalletSettings _creditWalletSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWalletService _walletService;
        private readonly IWorkContext _workContext;

        public CreditWalletCartBalanceViewComponent(
            CreditWalletSettings creditWalletSettings,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWalletService walletService,
            IWorkContext workContext)
        {
            _creditWalletSettings = creditWalletSettings;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _walletService = walletService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(object extraData)
        {
            if (!_creditWalletSettings.ShowAvailableCreditOnCheckoutPage)
                return Content("");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var wallet = await _walletService.GetWalletByCustomerIdAsync(customer.Id);
            var balance = await _priceFormatter.FormatPriceAsync((wallet?.Active ?? false) ? wallet.AvailableCredit : 0, true, wallet == null ? await _workContext.GetWorkingCurrencyAsync() : await _currencyService.GetCurrencyByIdAsync(wallet.CurrencyId));
            return View(model: balance);
        }
    }
}
