using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeKlarna.Models;

namespace NopStation.Plugin.Payments.StripeKlarna.Components
{
    public class StripeKlarnaViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly StripeKlarnaPaymentSettings _klarnaPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public StripeKlarnaViewComponent(IWorkContext workContext,
            StripeKlarnaPaymentSettings klarnaPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _klarnaPaymentSettings = klarnaPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_klarnaPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeKlarna/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
