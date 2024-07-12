using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeBoleto.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeBoletoWebhook", pattern + "stripeboleto/webhook",
                new { controller = "StripeBoleto", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeBoletoPayment", pattern + "stripeboleto/payment/{orderId}",
                new { controller = "StripeBoleto", action = "Payment" });

            endpointRouteBuilder.MapControllerRoute("StripeBoletoCallback", pattern + "stripeboleto/callback/{orderId}",
                new { controller = "StripeBoleto", action = "Callback" });
        }
    }
}
