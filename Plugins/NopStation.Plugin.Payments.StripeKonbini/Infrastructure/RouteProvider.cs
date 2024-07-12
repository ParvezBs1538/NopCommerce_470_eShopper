using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeKonbini.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeKonbiniWebhook", pattern + "/stripekonbini/webhook",
                new { controller = "StripeKonbini", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeKonbiniPayment", pattern + "/stripekonbini/payment/{orderId}",
                new { controller = "StripeKonbini", action = "Payment" });

            endpointRouteBuilder.MapControllerRoute("StripeKonbiniCallback", pattern + "/stripekonbini/callback/{orderId}",
                new { controller = "StripeKonbini", action = "Callback" });
        }
    }
}
