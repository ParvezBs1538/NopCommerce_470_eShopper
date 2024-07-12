using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.QuickView.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("QuickViewProductDetails", $"{pattern}quickview-product-details",
            new { controller = "QuickView", action = "ProductDetails" });
    }

    public int Priority
    {
        get { return 10; }
    }
}
