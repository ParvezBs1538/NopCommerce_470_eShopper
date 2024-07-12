using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute(name: "DMSShipments",
                pattern: $"{pattern}/dms/shipments",
                defaults: new { controller = "DMS", action = "Shipments" });

            endpointRouteBuilder.MapControllerRoute(name: "DMSShipmentDetails",
                pattern: $"{pattern}/dms/shipmentdetails/{{shipmentId:min(0)}}",
                defaults: new { controller = "DMS", action = "ShipmentDetails" });

            endpointRouteBuilder.MapControllerRoute(name: "DMSMarkAsShipped",
                pattern: $"{pattern}/dms/markasshipped/{{shipmentId:min(0)}}",
                defaults: new { controller = "DMS", action = "MarkAsShipped" });

            endpointRouteBuilder.MapControllerRoute(name: "DMSMarkAsDelivered",
                pattern: $"{pattern}/dms/markasdelivered/{{shipmentId:min(0)}}",
                defaults: new { controller = "DMS", action = "MarkAsDelivered" });
        }

        #endregion

        #region Properties

        public int Priority => 1;

        #endregion
    }
}
