using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Http.Extensions;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.NetsEasy.Models;
using NopStation.Plugin.Payments.NetsEasy.Services;

namespace NopStation.Plugin.Payments.NetsEasy.Components
{
    public class NetsEasyPaymentInfoViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly INetsEasyPaymentService _netsEasyPaymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly NetsEasyPaymentSettings _netsEasyPaymentSettings;

        public NetsEasyPaymentInfoViewComponent(IStoreContext storeContext,
            INetsEasyPaymentService netsEasyPaymentService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ILogger logger,
            NetsEasyPaymentSettings netsEasyPaymentSettings)
        {
            _storeContext = storeContext;
            _netsEasyPaymentService = netsEasyPaymentService;
            _localizationService = localizationService;
            _workContext = workContext;
            _logger = logger;
            _netsEasyPaymentSettings = netsEasyPaymentSettings;
        }

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //prepare order GUID
            var paymentRequest = new ProcessPaymentRequest();
            paymentRequest.OrderGuid = Guid.NewGuid();

            var model = new PublicInfoModel
            {
                CheckoutKey = _netsEasyPaymentSettings.CheckoutKey,
                CheckoutScriptUrl = _netsEasyPaymentSettings.TestMode ?
                        NetsEasyPaymentDefaults.TestCheckoutScriptUrl : NetsEasyPaymentDefaults.LiveCheckoutScriptUrl,
                CurrentLanguage = await GetCurrentLanguageAsync()
            };

            try
            {
                var paymentResponse = await _netsEasyPaymentService.CreatePaymentAsync(paymentRequest, (await _storeContext.GetCurrentStoreAsync()).Id);
                paymentRequest.CustomValues.Add(await _localizationService.GetResourceAsync("NopStation.NetsEasyPayment.PaymentId"), paymentResponse.PaymentId);

                await HttpContext.Session.SetAsync(NetsEasyPaymentDefaults.PaymentRequestSessionKey, paymentRequest);

                model.PaymentId = paymentResponse.PaymentId;

                return View(model);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("NetsEasy request failed.", ex);
                model.AddError(ex.Message);
                return View(model);
            }
        }

        #endregion

        #region Utility

        private async Task<string> GetCurrentLanguageAsync()
        {
            var lang = (await _workContext.GetWorkingLanguageAsync()).LanguageCulture;
            var supportedList = new List<string>()
            {
                "en-GB",
                "da-DK",
                "nl-NL",
                "fi-FI",
                "fr-FR",
                "de-DE",
                "nb-NO",
                "pl-PL",
                "es-ES",
                "sk-SK",
                "sv-SE",
            };

            if (supportedList.Contains(lang))
            {
                return lang;
            }
            return "en-GB";
        }

        #endregion

    }
}
