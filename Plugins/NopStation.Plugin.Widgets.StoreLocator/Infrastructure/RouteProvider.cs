using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.StoreLocator.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("StoreLocations", pattern + "store-locations",
                new { controller = "StoreLocation", action = "Stores" });

            endpointRouteBuilder.MapControllerRoute("StoreLocationStore", pattern + "store/{id:int}",
                new { controller = "StoreLocation", action = "Store" });
        }
    }
}
