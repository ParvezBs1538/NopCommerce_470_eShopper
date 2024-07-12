using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Widgets.StoreLocator.Infrastructure
{
    public class RoutingEventConsumer : IConsumer<GenericRoutingEvent>
    {
        public Task HandleEventAsync(GenericRoutingEvent eventMessage)
        {
            var values = eventMessage.RouteValues;
            var urlRecord = eventMessage.UrlRecord;

            if (urlRecord.EntityName.ToLowerInvariant() == "storelocation")
            {
                values[NopRoutingDefaults.RouteValue.Controller] = "StoreLocation";
                values[NopRoutingDefaults.RouteValue.Action] = "Store";
                values["storelocationid"] = urlRecord.EntityId;
                values[NopRoutingDefaults.RouteValue.SeName] = urlRecord.Slug;
            }

            return Task.CompletedTask;
        }
    }
}
