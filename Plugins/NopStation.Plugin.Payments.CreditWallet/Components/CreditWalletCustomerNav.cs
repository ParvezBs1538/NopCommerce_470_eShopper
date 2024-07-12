using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.CreditWallet.Services;

namespace NopStation.Plugin.Payments.CreditWallet.Components
{
    public class CreditWalletCustomerNavViewComponent : NopStationViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IWalletService _walletService;

        public CreditWalletCustomerNavViewComponent(IWorkContext workContext,
            IWalletService walletService)
        {
            _workContext = workContext;
            _walletService = walletService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var wallet = await _walletService.GetWalletByCustomerIdAsync(customer.Id);

            if (wallet == null || !wallet.Active)
                return Content("");

            return View();
        }
    }
}