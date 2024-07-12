using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            //product search
            endpointRouteBuilder.MapControllerRoute("AlgoliaSearch", $"{pattern}/search/",
                new { controller = "AlgoliaSearch", action = "Search" });
        }

        #endregion

        #region Properties

        public int Priority => 1;

        #endregion
    }
}
