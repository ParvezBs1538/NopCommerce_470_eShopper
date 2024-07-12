using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("NopStationCheckoutPaymentInfo", $"{pattern}nopstationcheckout/paymentinfo",
                new { controller = "NopStationCheckout", action = "PaymentInfo" });
            endpointRouteBuilder.MapControllerRoute("NopStationCheckoutRedirect", $"{pattern}nopstationcheckout/redirect",
                new { controller = "NopStationCheckout", action = "Redirect" });
            endpointRouteBuilder.MapControllerRoute("NopStationCheckoutStep", pattern + "nopstationcheckout/step/{nextStep}",
                new { controller = "NopStationCheckout", action = "Step" });
        }

        public int Priority => 0;
    }
}
