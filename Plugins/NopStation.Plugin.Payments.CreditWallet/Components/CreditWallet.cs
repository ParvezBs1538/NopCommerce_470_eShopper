using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.CreditWallet.Models;

namespace NopStation.Plugin.Payments.CreditWallet.Components
{
    public class CreditWalletViewComponent : NopStationViewComponent
    {
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly CreditWalletSettings _creditWalletSettings;
        private readonly IWorkContext _workContext;

        public CreditWalletViewComponent(ILocalizationService localizationService,
            IStoreContext storeContext,
            CreditWalletSettings creditWalletSettings,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _storeContext = storeContext;
            _creditWalletSettings = creditWalletSettings;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_creditWalletSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View(model);
        }
    }
}
