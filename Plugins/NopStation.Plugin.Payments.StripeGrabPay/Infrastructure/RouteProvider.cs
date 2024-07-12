using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeGrabPay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeGrabPayWebhook", pattern + "stripegrabpay/webhook",
                new { controller = "StripeGrabPay", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeGrabPayCallback", pattern + "stripegrabpay/callback/{orderId}",
                new { controller = "StripeGrabPay", action = "Callback" });
        }
    }
}
