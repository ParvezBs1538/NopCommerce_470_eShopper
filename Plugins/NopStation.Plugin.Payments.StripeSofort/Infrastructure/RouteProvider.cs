using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeSofort.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeSofortWebhook", pattern + "/stripesofort/webhook",
                new { controller = "StripeSofort", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeSofortCallback", pattern + "/stripesofort/callback/{orderId}",
                new { controller = "StripeSofort", action = "Callback" });
        }
    }
}
