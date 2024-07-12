using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.PayHere.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("PayHereReturn", pattern + "/payhere/return/{orderId}/{status}",
                 new { controller = "PayHere", action = "Return" });

            endpointRouteBuilder.MapControllerRoute("PayHereNotify", pattern + "/payhere/notify/{orderId}",
                 new { controller = "PayHere", action = "Notify" });
        }

        public int Priority => 0;
    }
}
