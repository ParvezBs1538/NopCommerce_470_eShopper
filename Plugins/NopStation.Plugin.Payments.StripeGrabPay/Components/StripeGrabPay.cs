﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeGrabPay.Models;
using Nop.Services.Localization;

namespace NopStation.Plugin.Payments.StripeGrabPay.Components
{
    public class StripeGrabPayViewComponent : NopStationViewComponent
    {
        #region Field

        private readonly IWorkContext _workContext;
        private readonly StripeGrabPayPaymentSettings _grabPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public StripeGrabPayViewComponent(IWorkContext workContext,
            StripeGrabPayPaymentSettings grabPayPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _grabPayPaymentSettings = grabPayPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods 

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = await _localizationService.GetLocalizedSettingAsync(_grabPayPaymentSettings,
                    x => x.DescriptionText, (await _workContext.GetWorkingLanguageAsync()).Id, (await _storeContext.GetCurrentStoreAsync()).Id)
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeGrabPay/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}