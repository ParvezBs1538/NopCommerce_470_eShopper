using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.OrderRatings.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("SaveOrderRatings", pattern + "saveorderratings",
                new { controller = "OrderRating", action = "SaveRatings" });

            endpointRouteBuilder.MapControllerRoute("SaveOrderRating", pattern + "saveorderrating",
                new { controller = "OrderRating", action = "SaveRating" });

            endpointRouteBuilder.MapControllerRoute("SkipOrderRatings", pattern + "skiporderratings",
                new { controller = "OrderRating", action = "SkipRatings" });

            endpointRouteBuilder.MapControllerRoute("LoadRateableOrders", pattern + "loadrateableorders",
                new { controller = "OrderRating", action = "LoadRateableOrders" });
        }

        public int Priority => int.MaxValue;
    }
}
