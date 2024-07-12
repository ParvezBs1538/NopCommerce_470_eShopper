using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Unzer.Models;

namespace NopStation.Plugin.Payments.Unzer.Components
{
    public class UnzerViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly UnzerPaymentSettings _unzerSettings;

        #endregion

        #region Ctor

        public UnzerViewComponent(UnzerPaymentSettings heidelpayPaymentSettings)
        {
            _unzerSettings = heidelpayPaymentSettings;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel
            {
                ApiPublicKey = _unzerSettings.ApiPublicKey,
                IsCardActive = _unzerSettings.IsCardActive,
                IsPaypalActive = _unzerSettings.IsPaypalActive,
                IsSofortActive = _unzerSettings.IsSofortActive,
                IsEpsActive = _unzerSettings.IsEpsActive,
            };

            return View("~/Plugins/NopStation.Plugin.Payments.Unzer/Views/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}