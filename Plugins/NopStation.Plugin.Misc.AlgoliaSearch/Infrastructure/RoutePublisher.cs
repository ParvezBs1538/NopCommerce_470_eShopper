using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure
{
    public class RoutePublisher : IRoutePublisher
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapHub<ProductUploadHub>("/uploadproducts");
        }
    }
}
