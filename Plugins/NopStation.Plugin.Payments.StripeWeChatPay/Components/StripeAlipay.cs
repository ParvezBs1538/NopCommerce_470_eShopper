using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeWeChatPay.Models;

namespace NopStation.Plugin.Payments.StripeWeChatPay.Components
{
    public class StripeWeChatPayViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly StripeWeChatPayPaymentSettings _weChatPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public StripeWeChatPayViewComponent(IWorkContext workContext,
            StripeWeChatPayPaymentSettings weChatPayPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _weChatPayPaymentSettings = weChatPayPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_weChatPayPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeWeChatPay/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}
