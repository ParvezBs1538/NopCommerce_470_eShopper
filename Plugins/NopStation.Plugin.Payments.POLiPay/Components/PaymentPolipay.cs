using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.POLiPay.Components
{
    public class PaymentPolipayViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
