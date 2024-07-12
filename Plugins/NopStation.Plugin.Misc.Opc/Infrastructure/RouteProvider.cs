using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Misc.Opc.Infrastructure;

public partial class RouteProvider : IRouteProvider
{
    public int Priority => 1;

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var pattern = string.Empty;

        if (DataSettingsManager.IsDatabaseInstalled())
        {
            var localizationSettings = endpointRouteBuilder.ServiceProvider.GetRequiredService<LocalizationSettings>();
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var langservice = endpointRouteBuilder.ServiceProvider.GetRequiredService<ILanguageService>();
                var languages = langservice.GetAllLanguagesAsync().Result.ToList();
                pattern = "{language:lang=" + languages.FirstOrDefault().UniqueSeoCode + "}/";
            }
        }

        endpointRouteBuilder.MapControllerRoute(name: "Opc",
            pattern: $"{pattern}order/checkout",
            defaults: new { controller = "Opc", action = "Checkout" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcSetBillingAddress",
            pattern: $"{pattern}opc/setbillingaddress",
            defaults: new { controller = "Opc", action = "SetBillingAddress" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcEditBillingAddress",
            pattern: $"{pattern}opc/editbillingaddress",
            defaults: new { controller = "Opc", action = "GetAddressById" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcSetShippingAddress",
            pattern: $"{pattern}opc/setshippingaddress",
            defaults: new { controller = "Opc", action = "SetShippingAddress" });


        endpointRouteBuilder.MapControllerRoute(name: "OpcUpdateBillingAddress",
            pattern: $"{pattern}opc/updateaddress",
            defaults: new { controller = "Opc", action = "UpdateBillingAddress" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcEditShippingAddress",
            pattern: $"{pattern}opc/editshippingaddress",
            defaults: new { controller = "Opc", action = "GetAddressById" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadPaymentMethod",
            pattern: $"{pattern}opc/loadpaymentmethods",
            defaults: new { controller = "Opc", action = "LoadPaymentMethods" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadPaymentMethodsByAddress",
            pattern: $"{pattern}Opc/GetPaymentMethodsByAddress",
            defaults: new { controller = "Opc", action = "GetPaymentMethodsByAddress" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcUseRewardPoints",
            pattern: $"{pattern}opc/userewardpoints",
            defaults: new { controller = "Opc", action = "UseRewardPoints" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcUpdatePayment",
            pattern: $"{pattern}opc/updatepayment",
            defaults: new { controller = "Opc", action = "UpdatePayment" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadPaymentInfo",
            pattern: $"{pattern}opc/loadpaymentinfo",
            defaults: new { controller = "Opc", action = "LoadPaymentInfo" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcGetEstimateShipping",
            pattern: $"{pattern}opc/getestimateshipping",
            defaults: new { controller = "Opc", action = "GetEstimateShipping" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcUpdateShippingAddress",
            pattern: $"{pattern}opc/updateshippingaddress",
            defaults: new { controller = "Opc", action = "UpdateShippingAddress" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcOrderReview",
           pattern: $"{pattern}opc/orderreview",
           defaults: new { controller = "Opc", action = "LoadOrderReview" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcConfirmOrder",
            pattern: $"{pattern}Opc/ConfirmOrder",
            defaults: new { controller = "Opc", action = "ConfirmOrder" });

        #region Shipping method

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadShippingMethod",
            pattern: $"{pattern}opc/loadshippingmethod",
            defaults: new { controller = "Opc", action = "LoadShippingMethod" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadShippingMethodByAddress",
            pattern: $"{pattern}opc/loadshippingmethodbyaddress",
            defaults: new { controller = "Opc", action = "LoadShippingMethodByAddress" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcUpdateShippingMethod",
            pattern: $"{pattern}opc/updateshippingmethod",
            defaults: new { controller = "Opc", action = "UpdateShippingMethod" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcSetPickUpInStore",
            pattern: $"{pattern}opc/setpickupinstore",
            defaults: new { controller = "Opc", action = "SetPickUpInStore" });

        #endregion Shipping method

        #region Shopping cart

        endpointRouteBuilder.MapControllerRoute(name: "OpcGetShoppingCartItems",
            pattern: $"{pattern}opc/getshoppingcartitems",
            defaults: new { controller = "Opc", action = "GetShoppingCartItems" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcUpdateShoppingCartItem",
            pattern: $"{pattern}opc/updateshoppingcartitem",
            defaults: new { controller = "Opc", action = "UpdateShoppingCartItem" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcDeleteShoppingCartItem",
            pattern: $"{pattern}opc/deleteshoppingcartitem",
            defaults: new { controller = "Opc", action = "DeleteShoppingCartItem" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcGetFlyOutCart",
            pattern: $"{pattern}opc/getflyoutcart",
            defaults: new { controller = "Opc", action = "GetFlyOutCart" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcGetCheckoutAttributes",
           pattern: $"{pattern}opc/loadcheckoutattributes",
           defaults: new { controller = "Opc", action = "LoadCheckoutAttributes" });

        #endregion Shopping cart

        #region Confirm order

        endpointRouteBuilder.MapControllerRoute(name: "OpcLoadOrderTotals",
            pattern: $"{pattern}opc/getordertotals",
            defaults: new { controller = "Opc", action = "GetOrderTotals" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcCompleted",
           pattern: $"{pattern}opc/completed/{{orderId:int?}}",
           defaults: new { controller = "Opc", action = "Completed" });

        #endregion Confirm order

        #region Gift card

        endpointRouteBuilder.MapControllerRoute(name: "OpcApplyGiftCard",
            pattern: $"{pattern}opc/applygiftcard",
            defaults: new { controller = "Opc", action = "ApplyGiftCard" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcRemoveGiftCard",
            pattern: $"{pattern}opc/removegiftcardcode",
            defaults: new { controller = "Opc", action = "RemoveGiftCardCode" });

        #endregion Gift card

        #region Discount

        endpointRouteBuilder.MapControllerRoute(name: "OpcApplyDiscount",
           pattern: $"{pattern}opc/applydiscountcoupon",
           defaults: new { controller = "Opc", action = "ApplyDiscountCoupon" });

        endpointRouteBuilder.MapControllerRoute(name: "OpcRemoveDiscount",
           pattern: $"{pattern}opc/removediscountcoupon",
           defaults: new { controller = "Opc", action = "RemoveDiscountCoupon" });

        #endregion Discount

        #region BuyNow

        endpointRouteBuilder.MapControllerRoute(name: "AddProductToCartCheckout-Catalog",
            pattern: $"addproducttocartcheckout/catalog/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}/{{quantity:min(0)}}",
            defaults: new { controller = "Opc", action = "AddProductToCartCheckout_Catalog" });

        endpointRouteBuilder.MapControllerRoute(name: "AddProductToCartCheckout-Details",
            pattern: $"addproducttocartcheckout/details/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}",
            defaults: new { controller = "Opc", action = "AddProductToCartCheckout_Details" });

        #endregion BuyNow
    }
}