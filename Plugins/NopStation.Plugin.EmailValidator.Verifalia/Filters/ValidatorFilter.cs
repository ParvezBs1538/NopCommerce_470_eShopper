using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.EmailValidator.Verifalia.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Customer;
using System.ComponentModel.DataAnnotations;

namespace NopStation.Plugin.EmailValidator.Verifalia.Filters
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
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.VerifaliaEmailValidator.EmailAddress.InvalidFormat").Result);
                return;
            }

            var verifaliaEmailValidatorSettings = NopInstance.Load<VerifaliaEmailValidatorSettings>();
            var domain = email.Split("@")[1];

            if (verifaliaEmailValidatorSettings.BlockedDomains.Contains(domain))
            {
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.VerifaliaEmailValidator.EmailAddress.InvalidDomain").Result);
                return;
            }

            var verifaliaEmailService = NopInstance.Load<IVerifaliaEmailService>();
            var verifaliaEmail = verifaliaEmailService.GetVerifaliaEmailByEmailAsync(email).Result;

            if (verifaliaEmail != null)
            {
                if (verifaliaEmail.IsValid(verifaliaEmailValidatorSettings.AllowRiskyEmails))
                    return;

                if (verifaliaEmail.UpdatedOnUtc.AddHours(verifaliaEmailValidatorSettings.RevalidateInvalidEmailsAfterHours) > DateTime.UtcNow)
                {
                    context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.VerifaliaEmailValidator.EmailAddress.InvalidEmail").Result);
                    return;
                }
            }

            var validation = NopInstance.Load<IValidationService>().ValidationEmailAsync(email).Result;
            var validationEntry = validation.Entries[0];
            validationEntry.SaveAsync(verifaliaEmailService, verifaliaEmail).Wait();

            if (validationEntry.IsValid(verifaliaEmailValidatorSettings.AllowRiskyEmails))
                return;

            context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.VerifaliaEmailValidator.EmailAddress.InvalidEmail").Result);
        }

        #endregion

        #region Methods

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var verifaliaEmailValidatorSettings = NopInstance.Load<VerifaliaEmailValidatorSettings>();
                if (!verifaliaEmailValidatorSettings.EnablePlugin)
                    return;

                if (!verifaliaEmailValidatorSettings.ValidateCustomerInfoEmail && !verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail)
                    return;

                var controller = context.Controller;
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = descriptor.ActionName;
                var method = context.HttpContext.Request.Method;

                if (controller.GetType() == typeof(CheckoutController) && (actionName == nameof(CheckoutController.OpcSaveShipping) ||
                    actionName == nameof(CheckoutController.ShippingAddress)) && method == "POST")
                {
                    if (!verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail)
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
                    if (!verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail)
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
                    if (!verifaliaEmailValidatorSettings.ValidateCustomerAddressEmail)
                        return;

                    var model = context.ActionArguments["model"] as CustomerAddressEditModel;
                    ValidateEmail(model.Address.Email, context);
                    return;
                }

                if (controller.GetType() == typeof(CustomerController) && method == "POST" &&
                    (actionName == nameof(CustomerController.Register) || actionName == nameof(CustomerController.Info)))
                {
                    if (!verifaliaEmailValidatorSettings.ValidateCustomerInfoEmail)
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
