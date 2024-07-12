using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Payments.Dmoney.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("DmoneyPay", pattern + "/checkout/dmoney/{transactionTrackingNo}",
                new { controller = "Dmoney", action = "Pay" });

            endpointRouteBuilder.MapControllerRoute("DmoneyApprovePayment", pattern + "/dmoney/approve/{transactionTrackingNo}",
                 new { controller = "Dmoney", action = "Approve" });

            endpointRouteBuilder.MapControllerRoute("DmoneyCancelPayment", pattern + "/dmoney/cancel/{transactionTrackingNo}",
                 new { controller = "Dmoney", action = "Cancel" });

            endpointRouteBuilder.MapControllerRoute("DmoneyDeclinePayment", pattern + "/dmoney/decline/{transactionTrackingNo}",
                 new { controller = "Dmoney", action = "Decline" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
