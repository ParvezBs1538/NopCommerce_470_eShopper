using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Misc.AjaxFilter.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 1000;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("AjaxFilterLoadFilters", "ajaxfilter/loadfilters",
                new { controller = "AjaxFilter", action = "LoadFilters" });

            endpointRouteBuilder.MapControllerRoute("AjaxFilterReloadFilters", "ajaxfilter/reloadfilters",
                new { controller = "AjaxFilter", action = "ReloadFilters" });
        }
    }
}
