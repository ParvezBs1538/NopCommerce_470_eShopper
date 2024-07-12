using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Razorpay.Models;

namespace NopStation.Plugin.Payments.Razorpay.Components
{
    public class RazorpayViewComponent : NopStationViewComponent
    {
        private readonly RazorpayPaymentSettings _razorpayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public RazorpayViewComponent(RazorpayPaymentSettings razorpayPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _razorpayPaymentSettings = razorpayPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                Description = await _localizationService.GetLocalizedSettingAsync(_razorpayPaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Razorpay/Views/PaymentInfo.cshtml", model);
        }
    }
}
