using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Flutterwave.Models;

namespace NopStation.Plugin.Payments.Flutterwave.Components
{
    public class FlutterwaveViewComponent : NopStationViewComponent
    {
        private readonly FlutterwavePaymentSettings _flutterwavePaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public FlutterwaveViewComponent(FlutterwavePaymentSettings flutterwavePaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _flutterwavePaymentSettings = flutterwavePaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                Description = await _localizationService.GetLocalizedSettingAsync(_flutterwavePaymentSettings,
                    x => x.Description, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Flutterwave/Views/PaymentInfo.cshtml", model);
        }
    }
}
