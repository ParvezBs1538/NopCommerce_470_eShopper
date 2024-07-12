using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.ProductBadge.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    #region Properties

    public int Priority => 1;

    #endregion

    #region Methods

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("ProductBadge", $"{pattern}load_badges",
            new { controller = "ProductBadge", action = "GetProductBadges" });
    }

    #endregion
}