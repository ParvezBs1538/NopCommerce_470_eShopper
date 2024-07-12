using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("DeliverySlots", pattern + "deliveryslots",
                new { controller = "DeliveryScheduler", action = "DeliverySlots" });

            endpointRouteBuilder.MapControllerRoute("SaveDeliverySlot", pattern + "savedeliveryslot",
                new { controller = "DeliveryScheduler", action = "SaveDeliverySlot" });
        }
    }
}
