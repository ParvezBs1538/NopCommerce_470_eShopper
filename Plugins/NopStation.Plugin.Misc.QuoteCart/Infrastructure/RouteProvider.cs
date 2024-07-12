using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure;

public class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var lang = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute("QuoteCart.Cart", $"{lang}/QuoteCart",
             new { controller = "QuoteCart", action = "Cart" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.UpdateCart", $"{lang}/UpdateQuoteCart",
             new { controller = "QuoteCart", action = "UpdateCart" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.AddQuote", $"{lang}/AddQuote",
             new { controller = "QuoteCart", action = "AddQuoteItem" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.AddQuote-Details", $"{lang}/addtoquotecart_details/{{productId}}",
             new { controller = "QuoteCart", action = "AddQuoteItem_Details" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.AddRequest", $"{lang}/QuoteRequest",
             new { controller = "QuoteRequest", action = "AddQuoteRequest" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.AttachmentUpload", $"{lang}QuoteAttachmentUpload",
             new { controller = "QuoteRequest", action = "AttachmentUpload" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.AllRequests", $"{lang}/customer/quoterequests",
             new { controller = "QuoteRequest", action = "AllRequest" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.RequestDetails", $"{lang}/quoterequestdetails/{{requestId}}",
             new { controller = "QuoteRequest", action = "Details" });
        endpointRouteBuilder.MapControllerRoute("QuoteCart.UploadFileFormAttribute", $"{lang}/uploadfileformattribute/{{attributeId}}",
             new { controller = "QuoteCart", action = "UploadFileFormAttribute" });
    }

    public int Priority => 0;
}