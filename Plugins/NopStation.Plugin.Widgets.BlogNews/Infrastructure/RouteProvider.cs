using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Widget.BlogNews.Infrastructure;

public record RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //throw new System.NotImplementedException();
    }

    public int Priority
    {
        get { return 0; }
    }
}