using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeIdeal.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeIdealWebhook", pattern + "/stripeideal/webhook",
                new { controller = "StripeIdeal", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeIdealCallback", pattern + "/stripeideal/callback/{orderId}",
                new { controller = "StripeIdeal", action = "Callback" });
        }
    }
}
