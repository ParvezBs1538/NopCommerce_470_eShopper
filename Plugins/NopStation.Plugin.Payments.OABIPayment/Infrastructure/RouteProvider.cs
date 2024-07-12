using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.OABIPayment.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            //return
            endpointRouteBuilder.MapControllerRoute("Plugin.NopStation.OABIPayment.Success",
                pattern + "/Plugins/OABIPayment/Success",
                new { controller = "OABIPayment", action = "Success" });

            endpointRouteBuilder.MapControllerRoute("Plugin.NopStation.OABIPayment.Fail",
                pattern + "/Plugins/OABIPayment/Fail",
                new { controller = "OABIPayment", action = "Fail" });
        }

        #endregion

        #region Properties

        public int Priority => 0;

        #endregion
    }
}
