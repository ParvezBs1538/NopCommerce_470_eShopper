using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Web.Controllers;

namespace NopStation.Plugin.Misc.Opc.Filters;

public class OpcActionFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context == null)
            return;

        var areaName = context.RouteData.Values["area"] as string;
        if (!string.IsNullOrWhiteSpace(areaName))
            return;

        var controller = context.Controller;
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var controllerType = controller.GetType();

        var opcSettings = EngineContext.Current.Resolve<OpcSettings>();

        if (opcSettings.EnableOnePageCheckout)
        {
            if (controllerType == typeof(CheckoutController) && !descriptor.ActionName.Equals("Completed", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Checkout", "Opc", null);
                return;
            }

            //if (opcSettings.BypassShoppingCartPage && controllerType == typeof(ShoppingCartController) &&
            //    descriptor.ActionName.Equals("Cart", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    var paymentPluginManager = EngineContext.Current.Resolve<IPaymentPluginManager>();
            //    var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            //    var workContext = EngineContext.Current.Resolve<IWorkContext>();
            //    var shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

            //    var store = storeContext.GetCurrentStore();
            //    var customer = workContext.GetCurrentCustomerAsync().Result;
            //    var cart = shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id).Result;

            //    var paymentMethods = paymentPluginManager
            //        .LoadActivePluginsAsync(customer, store.Id).Result
            //        .Where(pm => !pm.HidePaymentMethodAsync(cart).Result).ToList();

            //    var nonButtonPaymentMethods = paymentMethods
            //        .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
            //        .ToList();
            //    var buttonPaymentMethods = paymentMethods
            //        .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
            //        .ToList();
            //    if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
            //        return;

            //    context.Result = new RedirectToActionResult("Checkout", "Opc", null);
            //    return;
            //}
        }
    }
}