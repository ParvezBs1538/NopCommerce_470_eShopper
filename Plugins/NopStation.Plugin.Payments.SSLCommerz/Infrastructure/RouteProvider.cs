using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.SSLCommerz
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("SSLCommerz.Success", pattern + "sslcommerz/success/{orderGuid}",
                 new { controller = "SSLCommerz", action = "Success" });

            endpointRouteBuilder.MapControllerRoute("SSLCommerz.Failed", pattern + "sslcommerz/failed/{orderGuid}",
                 new { controller = "SSLCommerz", action = "Failed" });
        }

        public int Priority => -1;
    }
}
