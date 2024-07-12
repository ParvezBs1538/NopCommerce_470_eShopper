using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.AffiliateStation.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("AffiliateStation.AffiliateInfo", $"{pattern}affiliate/info",
                new { controller = "AffiliateStation", action = "Info" });

            endpointRouteBuilder.MapControllerRoute("AffiliateStation.AffiliatedOrders", $"{pattern}affiliatedorder/history",
                new { controller = "AffiliateStation", action = "Orders" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}