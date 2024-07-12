using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeKonbini.Models;

namespace NopStation.Plugin.Payments.StripeKonbini.Components
{
    public class StripeKonbiniViewComponent : NopStationViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel();
            var processPaymentRequest = await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest?.CustomValues?.TryGetValue(KonbiniDefaults.ConfirmationNumber, out var taxIdValue) ?? false)
                model.ConfirmationNumber = taxIdValue.ToString().ToLower();

            return View("~/Plugins/NopStation.Plugin.Payments.StripeKonbini/Views/PaymentInfo.cshtml", model);
        }
    }
}
