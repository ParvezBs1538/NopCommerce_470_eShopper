using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.ExternalAuth.Google.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(GoogleAuthenticationDefaults.DataDeletionCallbackRoute, $"google/data-deletion-callback/",
                new { controller = "GoogleDataDeletion", action = "DataDeletionCallback" });

            endpointRouteBuilder.MapControllerRoute(GoogleAuthenticationDefaults.DataDeletionStatusCheckRoute, $"google/data-deletion-status-check/{{earid:min(0)}}",
                new { controller = "GoogleAuthentication", action = "DataDeletionStatusCheck" });
        }

        public int Priority => 0;
    }
}
