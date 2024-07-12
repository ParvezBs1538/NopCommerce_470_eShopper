using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeAlipay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeAlipayWebhook", pattern + "stripealipay/webhook",
                new { controller = "StripeAlipay", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeAlipayCallback", pattern + "stripealipay/callback/{orderId}",
                new { controller = "StripeAlipay", action = "Callback" });
        }
    }
}
