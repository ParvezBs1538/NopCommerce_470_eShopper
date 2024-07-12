using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.PayHere.Models;

namespace NopStation.Plugin.Payments.PayHere.Components
{
    public class PayHereViewComponent : NopStationViewComponent
    {
        private readonly PayHerePaymentSettings _payHerePaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public PayHereViewComponent(PayHerePaymentSettings payHerePaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _payHerePaymentSettings = payHerePaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                Description = await _localizationService.GetLocalizedSettingAsync(_payHerePaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.PayHere/Views/PaymentInfo.cshtml", model);
        }
    }
}
