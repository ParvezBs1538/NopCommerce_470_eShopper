using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.Announcement.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    #region Methods

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("AnnouncementClose", $"{pattern}announcement/close",
            new { controller = "Announcement", action = "Close" });
        endpointRouteBuilder.MapControllerRoute("AnnouncementMinimize", $"{pattern}announcement/minimize",
            new { controller = "Announcement", action = "Minimize" });
    }

    #endregion

    #region Properties

    public int Priority => int.MaxValue;

    #endregion
}
