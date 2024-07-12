using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.StripeWeChatPay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StripeWeChatPayWebhook", pattern + "stripewechatpay/webhook",
                new { controller = "StripeWeChatPay", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute("StripeWeChatPayCallback", pattern + "stripewechatpay/callback/{orderId}",
                new { controller = "StripeWeChatPay", action = "Callback" });
        }
    }
}
