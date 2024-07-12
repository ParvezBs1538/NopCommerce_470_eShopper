using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    #region Methods

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("SmartDealCarousel", $"{pattern}load_smartdealcarousel_details",
            new { controller = "SmartDealCarousel", action = "Details" });
    }

    #endregion

    #region Properties

    public int Priority => int.MaxValue;

    #endregion
}
