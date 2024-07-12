using Microsoft.AspNetCore.Routing;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class CurrentRouteEvent
{
    #region Ctor

    public CurrentRouteEvent(RouteValueDictionary values)
    {
        RouteValues = values;
    }

    #endregion

    #region Properties

    public void SetRouteName(string routeName)
    {
        RouteName = routeName;
    }

    public RouteValueDictionary RouteValues { get; private set; }

    public string RouteName { get; private set; }

    #endregion
}