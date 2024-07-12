using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => 11;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("Recommendation", $"{pattern}load_recommendation_details",
                new { controller = "Recommendation", action = "Details" });
        }
    }
}