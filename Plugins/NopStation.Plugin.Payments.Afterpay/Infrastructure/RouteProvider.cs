using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.Afterpay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 10;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("AfterpayPostPaymentHandler", pattern + "/afterpay/postpaymenthandler",
                   new { controller = "AfterpayPayment", action = "PostPaymentHandler" });
            endpointRouteBuilder.MapControllerRoute("AfterpayCancelPayment", pattern + "/afterpay/cancelpayment",
                   new { controller = "AfterpayPayment", action = "CancelPayment" });
        }
    }
}
