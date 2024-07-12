using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.Paykeeper.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            //return
            endpointRouteBuilder.MapControllerRoute("Plugin.NopStation.Paykeeper.Return",
                pattern + "Plugins/PaykeeperWebHook/Return",
                new { controller = "PaykeeperWebHook", action = "Return" });

            endpointRouteBuilder.MapControllerRoute("Plugin.NopStation.Paykeeper.SuccessOrFail",
                pattern + "Plugins/PaykeeperWebHook/SuccessOrFail",
                new { controller = "PaykeeperWebHook", action = "SuccessOrFail" });
        }

        #endregion

        #region Properties

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}
