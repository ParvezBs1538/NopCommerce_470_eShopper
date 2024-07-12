using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.Helpdesk.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("HelpdeskReportBug", $"{pattern}/report-bug",
                new { controller = "HelpdeskTicket", action = "ReportBug" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskTickets", $"{pattern}/customer/mytickets",
                new { controller = "HelpdeskTicket", action = "List" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskAddNewTicket", $"{pattern}/customer/addticket",
                new { controller = "HelpdeskTicket", action = "AddNew" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskAddTicketResponse", $"{pattern}/customer/addresponse",
                new { controller = "HelpdeskTicket", action = "AddResponse" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskTicketDetails", pattern + "/customer/ticket/{id}",
                new { controller = "HelpdeskTicket", action = "Details" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskAddResponse", pattern + "/customer/addticketresponse",
                new { controller = "HelpdeskTicket", action = "AddResponse" });

            endpointRouteBuilder.MapControllerRoute("HelpdeskAttachment", $"{pattern}/customer/attachment",
                new { controller = "HelpdeskTicket", action = "DownloadFile" });
        }

        #endregion

        #region Properties

        public int Priority => 1;

        #endregion
    }
}