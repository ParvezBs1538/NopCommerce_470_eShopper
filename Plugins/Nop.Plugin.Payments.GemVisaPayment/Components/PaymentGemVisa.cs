using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.GemVisaPayment.Components
{
    [ViewComponent(Name = "PaymentGemVisa")]
    public class PaymentGemVisaViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.GemVisaPayment/Views/PaymentInfo.cshtml");
        }
    }
}
