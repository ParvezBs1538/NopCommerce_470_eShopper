using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Localization;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.UpayBdManual.Models;

namespace NopStation.Plugin.Payments.UpayBdManual.Components
{
    public class UpayBdManualViewComponent : NopStationViewComponent
    {
        private readonly UpayBdManualSettings _upayBdManualSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public UpayBdManualViewComponent(UpayBdManualSettings upayBdManualSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _upayBdManualSettings = upayBdManualSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_upayBdManualSettings,
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

            return View("~/Plugins/NopStation.Plugin.Payments.UpayBdManual/Views/PaymentInfo.cshtml", model);
        }
    }
}