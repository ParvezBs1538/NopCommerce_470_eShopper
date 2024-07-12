using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.Quickstream.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 10;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.QuickStream.Webhook", pattern + "QuickStream/Webhook",
                   new { controller = "QuickStreamPayment", action = "Webhook" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.QuickStream.postpaymenthandler", pattern + "QuickStream/postpaymenthandler",
                   new { controller = "QuickStreamPayment", action = "PostPaymentHandler" });
        }
    }
}
