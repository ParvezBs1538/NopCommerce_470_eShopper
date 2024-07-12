using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.EveryPay.Models;

namespace NopStation.Plugin.Payments.EveryPay.Components
{
    public class EveryPayPaymentScriptsViewComponent : NopStationViewComponent
    {
        private readonly EveryPaySettings _everyPaySettings;

        public EveryPayPaymentScriptsViewComponent(
            EveryPaySettings everyPaySettings)
        {
            _everyPaySettings = everyPaySettings;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            model.IsSandbox = _everyPaySettings.UseSandbox;
            return View("~/Plugins/NopStation.Plugin.Payments.EveryPay/Views/EveryPayPaymentScripts.cshtml", model);
        }
    }
}
