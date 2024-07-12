using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.iPayBd.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public int Priority => -1;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("iPayBdSuccess", pattern + "ipaybd/success/{orderGuid}",
                new { controller = "iPayBd", action = "Success" });

            endpointRouteBuilder.MapControllerRoute("iPayBdCancelled", pattern + "ipaybd/cancelled/{orderGuid}",
                new { controller = "iPayBd", action = "Cancelled" });

            endpointRouteBuilder.MapControllerRoute("iPayBdFailed", pattern + "ipaybd/failed/{orderGuid}",
                new { controller = "iPayBd", action = "Failed" });

            endpointRouteBuilder.MapControllerRoute("iPayBdProcess", pattern + "ipaybd/process/{orderGuid}",
                new { controller = "iPayBd", action = "Process" });
        }
    }
}
