using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.EmailValidator.Abstract.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Customer;
using System.ComponentModel.DataAnnotations;

namespace NopStation.Plugin.EmailValidator.Abstract.Filters
{
    public class ValidatorFilter : IActionFilter
    {
        #region Utilities

        protected void ValidateEmail(string email, ActionExecutingContext context)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            var attribute = new EmailAddressAttribute();

            if (!attribute.IsValid(email))
            {
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.AbstractEmailValidator.EmailAddress.InvalidFormat").Result);
                return;
            }

            var abstractEmailValidatorSettings = NopInstance.Load<AbstractEmailValidatorSettings>();
            var domain = email.Split("@")[1];

            if (abstractEmailValidatorSettings.BlockedDomains.Contains(domain))
            {
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.AbstractEmailValidator.EmailAddress.InvalidDomain").Result);
                return;
            }

            var abstractEmailService = NopInstance.Load<IAbstractEmailService>();
            var abstractEmail = abstractEmailService.GetAbstractEmailByEmailAsync(email).Result;

            if (abstractEmail != null)
            {
                if (abstractEmail.IsValid(abstractEmailValidatorSettings.AllowRiskyEmails))
                    return;

                if (abstractEmail.UpdatedOnUtc.AddHours(abstractEmailValidatorSettings.RevalidateInvalidEmailsAfterHours) > DateTime.UtcNow)
                {
                    context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.AbstractEmailValidator.EmailAddress.InvalidEmail").Result);
                    return;
                }
            }

            var apiResponse = NopInstance.Load<IValidationService>().ValidationEmailAsync(email).Result;
            apiResponse.SaveAsync(abstractEmailService, abstractEmail).Wait();

            if (apiResponse.IsValid(abstractEmailValidatorSettings.AllowRiskyEmails))
                return;

            context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.AbstractEmailValidator.EmailAddress.InvalidEmail").Result);
        }

        #endregion

        #region Methods

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var abstractEmailValidatorSettings = NopInstance.Load<AbstractEmailValidatorSettings>();
                if (!abstractEmailValidatorSettings.EnablePlugin)
                    return;

                if (!abstractEmailValidatorSettings.ValidateCustomerInfoEmail && !abstractEmailValidatorSettings.ValidateCustomerAddressEmail)
                    return;

                var controller = context.Controller;
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = descriptor.ActionName;
                var method = context.HttpContext.Request.Method;

                if (controller.GetType() == typeof(CheckoutController) && (actionName == nameof(CheckoutController.OpcSaveShipping) ||
                    actionName == nameof(CheckoutController.ShippingAddress)) && method == "POST")
                {
                    if (!abstractEmailValidatorSettings.ValidateCustomerAddressEmail)
                        return;

                    if (actionName == nameof(CheckoutController.OpcSaveShipping))
                    {
                        var form = context.ActionArguments["form"] as IFormCollection;
                        int.TryParse(form["shipping_address_id"], out var shippingAddressId);
                        if (shippingAddressId > 0)
                            return;
                    }

                    var model = context.ActionArguments["model"] as CheckoutShippingAddressModel;
                    ValidateEmail(model.ShippingNewAddress.Email, context);
                    return;
                }

                if (controller.GetType() == typeof(CheckoutController) && (actionName == nameof(CheckoutController.OpcSaveBilling) ||
                    actionName == nameof(CheckoutController.BillingAddress)) && method == "POST")
                {
                    if (!abstractEmailValidatorSettings.ValidateCustomerAddressEmail)
                        return;

                    if (actionName == nameof(CheckoutController.OpcSaveBilling))
                    {
                        var form = context.ActionArguments["form"] as IFormCollection;
                        int.TryParse(form["billing_address_id"], out var billingAddressId);
                        if (billingAddressId > 0)
                            return;
                    }

                    var model = context.ActionArguments["model"] as CheckoutBillingAddressModel;
                    ValidateEmail(model.BillingNewAddress.Email, context);
                    return;
                }

                if (controller.GetType() == typeof(CustomerController) && method == "POST" &&
                    (actionName == nameof(CustomerController.AddressAdd) || actionName == nameof(CustomerController.AddressEdit)))
                {
                    if (!abstractEmailValidatorSettings.ValidateCustomerAddressEmail)
                        return;

                    var model = context.ActionArguments["model"] as CustomerAddressEditModel;
                    ValidateEmail(model.Address.Email, context);
                    return;
                }

                if (controller.GetType() == typeof(CustomerController) && method == "POST" &&
                    (actionName == nameof(CustomerController.Register) || actionName == nameof(CustomerController.Info)))
                {
                    if (!abstractEmailValidatorSettings.ValidateCustomerInfoEmail)
                        return;

                    string email;
                    if (context.ActionArguments["model"].GetType() == typeof(RegisterModel))
                        email = (context.ActionArguments["model"] as RegisterModel).Email;
                    else
                        email = (context.ActionArguments["model"] as CustomerInfoModel).Email;

                    ValidateEmail(email, context);
                    return;
                }
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        #endregion
    }
}
