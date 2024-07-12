using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Paystack.Models;

namespace NopStation.Plugin.Payments.Paystack.Components
{
    public class PaystackViewComponent : NopStationViewComponent
    {
        private readonly PaystackPaymentSettings _paystackPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public PaystackViewComponent(PaystackPaymentSettings paystackPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _paystackPaymentSettings = paystackPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                Description = await _localizationService.GetLocalizedSettingAsync(_paystackPaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Paystack/Views/PaymentInfo.cshtml", model);
        }
    }
}
