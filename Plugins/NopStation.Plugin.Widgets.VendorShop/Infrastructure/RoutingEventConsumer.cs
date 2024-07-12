using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Infrastructure
{
    public class RoutingEventConsumer : IConsumer<GenericRoutingEvent>
    {
        private readonly IVendorShopFeatureService _vendorShopFeatureService;

        public RoutingEventConsumer(IVendorShopFeatureService vendorShopFeatureService)
        {
            _vendorShopFeatureService = vendorShopFeatureService;
        }
        public async Task HandleEventAsync(GenericRoutingEvent eventMessage)
        {
            var values = eventMessage.RouteValues;
            var urlRecord = eventMessage.UrlRecord;

            if (urlRecord.EntityName.ToLowerInvariant() == "vendor" && await _vendorShopFeatureService.IsEnableVendorShopAsync(urlRecord.EntityId))
            {
                values[NopRoutingDefaults.RouteValue.Controller] = "CatalogExtension";
                values[NopRoutingDefaults.RouteValue.Action] = "Vendor";
                values["vendorId"] = urlRecord.EntityId;
                values[NopRoutingDefaults.RouteValue.SeName] = urlRecord.Slug;

                eventMessage.Handled = true;
            }
        }
    }
}
