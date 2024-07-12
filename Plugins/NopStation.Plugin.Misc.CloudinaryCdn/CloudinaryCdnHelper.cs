using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.CloudinaryCdn
{
    public class CloudinaryCdnHelper
    {
        public static bool IsAdminArea()
        {
            var httpContextAccessor = NopInstance.Load<IHttpContextAccessor>();

            var routeData = httpContextAccessor.HttpContext.GetRouteData().Values["Area"];
            if (routeData != null && routeData.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }
    }
}