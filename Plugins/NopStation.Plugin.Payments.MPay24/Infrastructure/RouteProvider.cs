using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Payments.MPay24.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routes)
        {
            //IPN
            routes.MapControllerRoute("Plugin.Payments.MPay24.IPNHandler",
                 "Plugins/PaymentMPay24/IPNHandler",
                 new { controller = "PaymentMPay24", action = "IPNHandler" }
            );

            routes.MapControllerRoute("Plugin.Payments.MPay24.PDTHandler",
                 "Plugins/PaymentMPay24/PDTHandler",
                 new { controller = "PaymentMPay24", action = "PDTHandler" }
            );

            routes.MapControllerRoute("Plugin.Payments.MPay24.CancelOrder",
                 "Plugins/PaymentMPay24/CancelOrder",
                 new { controller = "PaymentMPay24", action = "CancelOrder" }
            );
        }

        public int Priority => -1;
    }
}
