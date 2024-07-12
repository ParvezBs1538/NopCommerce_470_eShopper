using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.POLiPay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("NopStation.Payments.PoliPay.PaymentPoliPay", $"{pattern}polipay/postpaymenthandler",
                   new { controller = "PaymentPoliPay", action = "PostPaymentHandler" });
            endpointRouteBuilder.MapControllerRoute("NopStation.Payments.PoliPay.PaymentPoliPay", $"{pattern}polipay/webhook",
                   new { controller = "PaymentPoliPay", action = "Webhook" });
        }
    }
}
