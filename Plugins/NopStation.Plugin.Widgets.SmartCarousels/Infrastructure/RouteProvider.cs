using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.SmartCarousels.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    #region Methods

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("SmartCarousel", $"{pattern}load_smartcarousel_details",
            new { controller = "SmartCarousel", action = "Details" });
    }

    #endregion

    #region Properties

    public int Priority => int.MaxValue;

    #endregion
}
