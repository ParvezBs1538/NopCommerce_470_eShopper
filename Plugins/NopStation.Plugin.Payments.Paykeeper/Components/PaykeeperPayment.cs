using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Paykeeper.Models;

namespace NopStation.Plugin.Payments.Paykeeper.Components
{
    public class PaykeeperPaymentViewComponent : NopStationViewComponent
    {
        private readonly PaykeeperPaymentSettings _paykeeperPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public PaykeeperPaymentViewComponent(PaykeeperPaymentSettings paykeeperPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _paykeeperPaymentSettings = paykeeperPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_paykeeperPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Paykeeper/Views/PaymentInfo.cshtml", model);
        }
    }
}
