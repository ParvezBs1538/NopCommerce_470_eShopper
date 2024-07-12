using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeOxxo.Models;

namespace NopStation.Plugin.Payments.StripeOxxo.Components
{
    public class StripeOxxoViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly StripeOxxoPaymentSettings _oxxoPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public StripeOxxoViewComponent(IWorkContext workContext,
            StripeOxxoPaymentSettings oxxoPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _oxxoPaymentSettings = oxxoPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_oxxoPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeOxxo/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
