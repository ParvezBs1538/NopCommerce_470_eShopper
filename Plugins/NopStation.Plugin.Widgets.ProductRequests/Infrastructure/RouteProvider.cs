using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Widgets.ProductRequests.Infrastructure
{
    public partial class RouteProvider : BaseRouteProvider, IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var pattern = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("AddProductRequest", $"{pattern}productrequest/addnew",
                new { controller = "ProductRequest", action = "AddNew" });

            endpointRouteBuilder.MapControllerRoute("ProductRequests", $"{pattern}productrequest/history",
                new { controller = "ProductRequest", action = "History" });

            endpointRouteBuilder.MapControllerRoute("ProductRequestDetails", $"{pattern}productrequest/details/{{id:min(0)}}",
                new { controller = "ProductRequest", action = "Details" });
        }

        #endregion

        #region Properties

        public int Priority => 1;

        #endregion
    }
}
