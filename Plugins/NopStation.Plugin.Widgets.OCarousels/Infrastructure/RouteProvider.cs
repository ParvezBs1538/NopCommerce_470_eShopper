using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.OCarousels.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("OCarousel", $"{pattern}load_carousel_details",
                new { controller = "OCarousel", action = "Details" });
        }

        #endregion

        #region Properties

        public int Priority => 1;

        #endregion
    }
}
