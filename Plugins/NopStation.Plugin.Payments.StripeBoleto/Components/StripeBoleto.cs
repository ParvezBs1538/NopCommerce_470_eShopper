using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.StripeBoleto.Models;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Payments;
using System.Threading.Tasks;

namespace NopStation.Plugin.Payments.StripeBoleto.Components
{
    public class StripeBoletoViewComponent : NopStationViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new PaymentInfoModel();
            var processPaymentRequest = await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest?.CustomValues?.TryGetValue(BoletoDefaults.TaxID, out var taxIdValue) ?? false)
                model.TaxID = taxIdValue.ToString().ToLower();

            return View("~/Plugins/NopStation.Plugin.Payments.StripeBoleto/Views/PaymentInfo.cshtml", model);
        }
    }
}
