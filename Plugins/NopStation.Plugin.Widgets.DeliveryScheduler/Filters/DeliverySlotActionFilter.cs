using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Controllers;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Filters
{
    public class DeliverySlotActionFilter : IActionFilter
    {
        protected virtual bool ParsePickupInStore(IFormCollection form)
        {
            var pickupInStore = false;

            var pickupInStoreParameter = form["PickupInStore"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pickupInStoreParameter))
                bool.TryParse(pickupInStoreParameter, out pickupInStore);

            return pickupInStore;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        protected virtual bool PluginActive()
        {
            var deliverySchedulerSettings = NopInstance.Load<DeliverySchedulerSettings>();
            if (!deliverySchedulerSettings.EnableScheduling)
                return false;

            return NopPlugin.IsEnabledAsync<IWidgetPlugin>(DeliverySchedulerDefaults.PluginSystemName).Result;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null || context.Controller.GetType() != typeof(CheckoutController))
                return;
            
            var actionName = ((ControllerBase)context.Controller).ControllerContext.ActionDescriptor.ActionName.ToLower();
            if (actionName != "opcsaveshippingmethod" && actionName != "shippingmethod")
                return;

            var methodType = context.HttpContext.Request.Method.ToLower();
            if (actionName == "shippingmethod" && !methodType.Equals("post"))
                return;

            if (!PluginActive())
                return;

            var form = context.HttpContext.Request.Form;
            var pickupInStore = ParsePickupInStore(form);
            if (pickupInStore)
                return;

            string shippingoption = form["shippingoption"];
            if (string.IsNullOrEmpty(shippingoption))
                return;

            var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedOption.Length != 2)
                return;
            if (splittedOption[1] != DeliverySchedulerDefaults.ShippingProviderName)
                return;

            var selectedName = splittedOption[0];
            var customShippingService = NopInstance.Load<ICustomShippingService>();
            var shippingMethod = customShippingService.GetShippingMethodByNameAsync(selectedName).Result;
            if (shippingMethod == null)
            {
                ShowError(actionName, context);
                return;
            }

            string savedSlotInfo = form["deliveryslot"];
            if (string.IsNullOrWhiteSpace(savedSlotInfo))
            {
                ShowError(actionName, context);
                return;
            }
            var tokens = savedSlotInfo.Split("___", StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length != 2)
            {
                ShowError(actionName, context);
                return;
            }

            var deliverySlotService = NopInstance.Load<IDeliverySlotService>();
            if (!int.TryParse(tokens[1], out var deliverySlotId) || deliverySlotService.GetDeliverySlotByIdAsync(deliverySlotId).Result is not DeliverySlot deliverySlot || 
                deliverySlot.Deleted || !deliverySlot.Active)
            {
                ShowError(actionName, context);
                return;
            }

            if (!DateTime.TryParse(tokens[0], out var deliveryDate) || deliveryDate.Date < DateTime.Now.Date)
            {
                ShowError(actionName, context);
                return;
            }

            var deliveryCapacityService = NopInstance.Load<IDeliveryCapacityService>();
            var capacity = deliveryCapacityService.GetDeliveryCapacityForDateSlotAsync(deliveryDate.Date, deliverySlot, shippingMethod.Id).Result;
            if (capacity < 1)
            {
                ShowError(actionName, context);
                return;
            }

            savedSlotInfo = $"{deliveryDate}___{deliverySlot.Id}___{shippingMethod.Id}";
            var genericAttributeService = NopInstance.Load<IGenericAttributeService>();
            var workContext = NopInstance.Load<IWorkContext>();
            var storeContext = NopInstance.Load<IStoreContext>();

            genericAttributeService.SaveAttributeAsync(workContext.GetCurrentCustomerAsync().Result,
                DeliverySchedulerDefaults.DeliverySlotInfo, savedSlotInfo, storeContext.GetCurrentStore().Id).Wait();
        }

        private static void ShowError(string actionName, ActionExecutingContext context)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            if (actionName == "opcsaveshippingmethod")
                context.Result = new JsonResult(new { error = 1, message = localizationService.GetResourceAsync("NopStation.DeliveryScheduler.Slots.NotSelected").Result });
            else
            {
                var notificationService = NopInstance.Load<INotificationService>();
                notificationService.ErrorNotification(localizationService.GetResourceAsync("NopStation.DeliveryScheduler.Slots.NotSelected").Result);
                context.Result = new RedirectToRouteResult("CheckoutShippingMethod", new { });
            }
        }
    }
}
