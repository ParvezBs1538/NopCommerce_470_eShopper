using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.AddressValidator.Byteplant.Services;
using NopStation.Plugin.AddressValidator.Byteplant.Services.Models;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Services.Attributes;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.AddressValidator.Byteplant.Filters
{
    public class ByteplantAddressValidatorFilter : IActionFilter
    {
        #region Utilities

        protected void ValidatePhoneNumber(ModelStateDictionary modelState, string phoneNumber, string phoneNumberRegex, ILocalizationService localizationService)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return;

            var match = Regex.Match(phoneNumber, phoneNumberRegex, RegexOptions.IgnoreCase);

            if (!match.Success)
                modelState.AddModelError("", localizationService.GetResourceAsync("NopStation.ByteplantAddressValidator.Common.InvalidPhoneNumber").Result);
        }

        protected void CheckNewShippingAddress(ActionExecutingContext context, ByteplantAddressValidatorSettings addressValidatorSettings)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();

            var model = context.ActionArguments["model"] as CheckoutShippingAddressModel;
            var response = GetResponse(model.ShippingNewAddress, context);
            if (response.Status != "VALID")
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.ByteplantAddressValidator.Checkout.InvalidAddress").Result);

            if (addressValidatorSettings.ValidatePhoneNumber)
                ValidatePhoneNumber(context.ModelState, model.ShippingNewAddress.PhoneNumber, addressValidatorSettings.PhoneNumberRegex, localizationService);
        }

        protected void CheckNewBillingAddress(ActionExecutingContext context, ByteplantAddressValidatorSettings addressValidatorSettings)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();

            var model = context.ActionArguments["model"] as CheckoutBillingAddressModel;

            var response = GetResponse(model.BillingNewAddress, context);
            if (response.Status != "VALID")
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.ByteplantAddressValidator.Checkout.InvalidAddress").Result);

            if (addressValidatorSettings.ValidatePhoneNumber)
                ValidatePhoneNumber(context.ModelState, model.BillingNewAddress.PhoneNumber, addressValidatorSettings.PhoneNumberRegex, localizationService);
        }

        protected void CheckCustomerAddress(ActionExecutingContext context, ByteplantAddressValidatorSettings addressValidatorSettings)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();

            var model = context.ActionArguments["model"] as CustomerAddressEditModel;
            var response = GetResponse(model.Address, context);
            if (response.Status != "VALID")
                context.ModelState.AddModelError("", localizationService.GetResourceAsync("NopStation.ByteplantAddressValidator.Customer.InvalidAddress").Result);

            if (addressValidatorSettings.ValidatePhoneNumber)
                ValidatePhoneNumber(context.ModelState, model.Address.PhoneNumber, addressValidatorSettings.PhoneNumberRegex, localizationService);
        }

        protected GeocodingResponse GetResponse(AddressModel address, ActionExecutingContext context)
        {
            var addressExtensionService = NopInstance.Load<IAddressExtensionService>();
            var addressAttributeParser = NopInstance.Load<IAttributeParser<AddressAttribute, AddressAttributeValue>>();

            var form = context.ActionArguments["form"] as IFormCollection;
            var customAttributes = addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName).Result;
            var response = addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.City,
                address.StateProvinceId, address.CountryId, customAttributes).Result;
            return response;
        }

        #endregion

        #region Methods

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var addressValidatorSettings = NopInstance.Load<ByteplantAddressValidatorSettings>();
                if (!addressValidatorSettings.EnablePlugin)
                    return;

                var controller = context.Controller;
                var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = descriptor.ActionName;
                var method = context.HttpContext.Request.Method;

                if (controller.GetType() == typeof(CheckoutController) && (actionName == "OpcSaveShipping" ||
                    actionName == "ShippingAddress") && method == "POST")
                {
                    if (actionName == "OpcSaveShipping")
                    {
                        var form = context.ActionArguments["form"] as IFormCollection;
                        int.TryParse(form["shipping_address_id"], out var shippingAddressId);
                        if (shippingAddressId > 0)
                            return;
                    }

                    CheckNewShippingAddress(context, addressValidatorSettings);
                    return;
                }

                if (controller.GetType() == typeof(CheckoutController) && (actionName == "OpcSaveBilling" ||
                    actionName == "BillingAddress") && method == "POST")
                {
                    if (actionName == "OpcSaveBilling")
                    {
                        var form = context.ActionArguments["form"] as IFormCollection;
                        int.TryParse(form["billing_address_id"], out var billingAddressId);
                        if (billingAddressId > 0)
                            return;
                    }

                    CheckNewBillingAddress(context, addressValidatorSettings);
                    return;
                }

                if (controller.GetType() == typeof(CustomerController) && method == "POST" &&
                    (actionName == "AddressAdd" || actionName == "AddressEdit"))
                {
                    CheckCustomerAddress(context, addressValidatorSettings);
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
