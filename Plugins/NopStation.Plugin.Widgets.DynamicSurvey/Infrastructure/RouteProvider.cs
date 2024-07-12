using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1000001;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var lang = GetLanguageRoutePattern();

            //generic routes
            var pattern = $"{lang}/{{SeName}}";

            endpointRouteBuilder.MapControllerRoute("DynamicSurvey", pattern,
                new { controller = "Survey", action = "SurveyDetails" });
        }
    }
}
