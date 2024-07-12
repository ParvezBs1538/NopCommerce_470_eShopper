using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;
using static Nop.Web.Framework.Mvc.Routing.NopRoutingDefaults;

namespace NopStation.Plugin.Widgets.Flipbooks.Infrastructure
{
    public class RoutingEventConsumer : IConsumer<GenericRoutingEvent>
    {
        public Task HandleEventAsync(GenericRoutingEvent eventMessage)
        {
            var values = eventMessage.RouteValues;
            var urlRecord = eventMessage.UrlRecord;

            if (urlRecord.EntityName.ToLowerInvariant() == "flipbook")
            {
                values[RouteValue.Controller] = "Flipbook";
                values[RouteValue.Action] = "Details";
                values["flipbookid"] = urlRecord.EntityId;
                values[RouteValue.SeName] = urlRecord.Slug;
            }

            return Task.CompletedTask;
        }
    }
}
