using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.Flutterwave.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("FlutterwaveReturn", pattern + "/flutterwave/return/{orderId}",
                 new { controller = "Flutterwave", action = "Return" });

            endpointRouteBuilder.MapControllerRoute("FlutterwaveNotify", pattern + "/flutterwave/notify/{orderId}",
                 new { controller = "Flutterwave", action = "Notify" });
        }

        public int Priority => 0;
    }
}
