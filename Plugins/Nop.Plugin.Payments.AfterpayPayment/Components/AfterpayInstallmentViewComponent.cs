using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.AfterpayPayment.Components
{
    [ViewComponent(Name = "AfterpayInstallment")]
    public class AfterpayInstallmentViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var price = (decimal)additionalData;
            return View("~/Plugins/Payments.AfterpayPayment/Views/AfterpayInstallment.cshtml", price);
        }
    }
}
