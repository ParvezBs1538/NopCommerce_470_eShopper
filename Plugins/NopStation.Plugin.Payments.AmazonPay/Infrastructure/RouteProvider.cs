using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.AmazonPay.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("AmazonPayCallback", pattern + "/amazonpay/callback/{orderGuid}",
                new { controller = "AmazonPay", action = "Callback" });
            endpointRouteBuilder.MapControllerRoute("AmazonPayRedirect", pattern + "/amazonpay/redirect/{orderGuid}",
                new { controller = "AmazonPay", action = "Redirect" });
        }
    }
}
