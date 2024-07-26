using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.GemVisaPayment.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 10;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.GemVisa.PaymentGemVisa", "gemvisa/postpaymenthandler",
                   new { controller = "PaymentGemVisa", action = "PostPaymentHandler" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.GemVisa.PaymentGemVisa", "gemvisa/webhook",
                   new { controller = "PaymentGemVisa", action = "Webhook" });
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.GemVisa.PaymentGemVisa", "gemvisa/rejectpayment",
                   new { controller = "PaymentGemVisa", action = "rejectpayment" });
        }
    }
}
