using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.bKash.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("BkashPay", pattern + "/checkout/bkashpay/{orderId:int}",
                new { controller = "Bkash", action = "Pay" });

            endpointRouteBuilder.MapControllerRoute("BkashCreatePayment", pattern + "/checkout/createbkashpayment",
                new { controller = "Bkash", action = "CreatePayment" });

            endpointRouteBuilder.MapControllerRoute("BkashExecutePayment", pattern + "/checkout/executebkashpayment",
                new { controller = "Bkash", action = "ExecutePayment" });
        }
    }
}
