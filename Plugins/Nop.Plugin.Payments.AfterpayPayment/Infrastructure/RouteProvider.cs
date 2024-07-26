using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.AfterpayPayment.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 10;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.AfterpayPayment.PaymentAfterpay", "postpaymenthandler",
                   new { controller = "AfterpayPayment", action = "PostPaymentHandler" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.AfterpayPayment.CancelPayment", "checkout/cancel",
                   new { controller = "AfterpayPayment", action = "CancelPayment" });
        }
    }
}
