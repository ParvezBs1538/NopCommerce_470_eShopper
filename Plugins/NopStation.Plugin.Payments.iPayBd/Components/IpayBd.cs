using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.iPayBd.Models;
using Nop.Services.Localization;

namespace NopStation.Plugin.Payments.iPayBd.Components
{
    public class IpayBdViewComponent : NopStationViewComponent
    {
        private readonly IpayBdPaymentSettings _ipayBdPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public IpayBdViewComponent(IpayBdPaymentSettings ipayBdPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _ipayBdPaymentSettings = ipayBdPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_ipayBdPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.iPayBd/Views/PaymentInfo.cshtml", model);
        }
    }
}