using System.Threading.Tasks;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Infrastructure
{
    public class RoutingEventConsumer : IConsumer<GenericRoutingEvent>
    {
        public Task HandleEventAsync(GenericRoutingEvent eventMessage)
        {
            var values = eventMessage.RouteValues;
            var urlRecord = eventMessage.UrlRecord;

            if (urlRecord.EntityName.ToLowerInvariant() == typeof(Survey).Name.ToLowerInvariant())
            {
                values[NopRoutingDefaults.RouteValue.Controller] = "Survey";
                values[NopRoutingDefaults.RouteValue.Action] = "SurveyDetails";
                values["surveyId"] = urlRecord.EntityId;
                values[NopRoutingDefaults.RouteValue.SeName] = urlRecord.Slug;
            }

            return Task.CompletedTask;
        }
    }
}
