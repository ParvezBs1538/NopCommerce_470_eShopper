using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Payments.OABIPayment.Components
{
    public class OABIPaymentViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
