using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Localization;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.RocketManual.Models;

namespace NopStation.Plugin.Payments.RocketManual.Components
{
    public class RocketManualViewComponent : NopStationViewComponent
    {
        private readonly RocketManualPaymentSettings _rocketManualPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public RocketManualViewComponent(RocketManualPaymentSettings rocketManualPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _rocketManualPaymentSettings = rocketManualPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_rocketManualPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            var processPaymentRequest = await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest != null)
            {
                if (processPaymentRequest.CustomValues.ContainsKey("Phone Number"))
                    model.PhoneNumber = processPaymentRequest.CustomValues["Phone Number"].ToString();
                if (processPaymentRequest.CustomValues.ContainsKey("Transaction Id"))
                    model.TransactionId = processPaymentRequest.CustomValues["Transaction Id"].ToString();
            }

            return View("~/Plugins/NopStation.Plugin.Payments.RocketManual/Views/PaymentInfo.cshtml", model);
        }
    }
}