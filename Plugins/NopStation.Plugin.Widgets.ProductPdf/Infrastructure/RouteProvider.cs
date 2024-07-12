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
            var lang = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute(name: "DownloadProductPdf",
                pattern: $"{lang}/downloadproductpdf/{{productId:min(0)}}",
                defaults: new { controller = "ProductPdf", action = "Pdf" });
        }

        #endregion

        #region Properties

        public int Priority => 11;

        #endregion
    }
}
