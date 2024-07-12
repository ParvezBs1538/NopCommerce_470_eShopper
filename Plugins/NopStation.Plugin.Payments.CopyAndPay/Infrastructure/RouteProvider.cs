using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.CopyAndPay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("CopyAndPayValidatePayment", pattern + "copyandpay/validatepayment/{orderId}",
              new { controller = "PaymentCopyAndPay", action = "ValidatePayment" });

            endpointRouteBuilder.MapControllerRoute("CopyAndPayPayment", pattern + "copyandpay/payment/{orderId}",
                new { controller = "PaymentCopyAndPay", action = "Payment" });
        }

        public int Priority => 1;
    }
}