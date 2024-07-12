using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeOxxo.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeOxxoWebhook", pattern + "/stripeoxxo/webhook",
                new { controller = "StripeOxxo", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeOxxoPayment", pattern + "/stripeoxxo/payment/{orderId}",
                new { controller = "StripeOxxo", action = "Payment" });

            endpointRouteBuilder.MapControllerRoute("StripeOxxoCallback", pattern + "/stripeoxxo/callback/{orderId}",
                new { controller = "StripeOxxo", action = "Callback" });
        }
    }
}
