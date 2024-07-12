using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeKlarna.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeKlarnaWebhook", pattern + "/stripeklarna/webhook",
                new { controller = "StripeKlarna", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeKlarnaCallback", pattern + "/stripeklarna/callback/{orderId}",
                new { controller = "StripeKlarna", action = "Callback" });
        }
    }
}
