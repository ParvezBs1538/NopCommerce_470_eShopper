using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.AddressValidator.EasyPost.Services;
using NopStation.Plugin.AddressValidator.EasyPost.Services.Models;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Web.Controllers;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;

namespace NopStation.Plugin.AddressValidator.EasyPost.Filters
{
    public class EasyPostAddressValidatorFilter : IActionFilter
    {
        #region Utilities

        protected void CheckNewShippingAddress(ActionExecutingContext context, EasyPostAddressValidatorSettings addressValidatorSettings)
        {
            var model = context.ActionArguments["model"] as CheckoutShippingAddressModel;
            var response = GetResponse(model.ShippingNewAddress, context);
            if (!response.Verification.Delivery.Success)
            {
                foreach (var item in response.Verification.Delivery.Errors)
                {
                    context.ModelState.AddModelError("", item.Message);
                }
            }
        }

        protected void CheckNewBillingAddress(ActionExecutingContext context, EasyPostAddressValidatorSettings addressValidatorSettings)
        {
            var model = context.ActionArguments["model"] as CheckoutBillingAddressModel;
            var response = GetResponse(model.BillingNewAddress, context);
            if (!response.Verification.Delivery.Success)
            {
                foreach (var item in response.Verification.Delivery.Errors)
                {
                    context.ModelState.AddModelError("", item.Message);
                }
            }
        }

        protected void CheckCustomerAddress(ActionExecutingContext context, EasyPostAddressValidatorSettings addressValidatorSettings)
        {
            var model = context.ActionArguments["model"] as CustomerAddressEditModel;
            var response = GetResponse(model.Address, context);
            if (!response.Verification.Delivery.Success)
            {
                foreach (var item in response.Verification.Delivery.Errors)
                {
                    context.ModelState.AddModelError("", item.Message);
                }
            }
        }

        protected GeocodingResponse GetResponse(AddressModel address, ActionExecutingContext context)
        {
            var addressExtensionService = NopInstance.Load<IAddressExtensionService>();
            var addressAttributeParser = NopInstance.Load<IAttributeParser<AddressAttribute, AddressAttributeValue>>();

            var form = context.ActionArguments["form"] as IFormCollection;
            var customAttributes = addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName).Result;
            var response = addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.Address2, 
                address.PhoneNumber, address.City, address.StateProvinceId, address.CountryId, address.Company, customAttributes).Result;
            return response;
        }

        #endregion

        #region Methods

        public void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var addressValidatorSettings = NopInstance.Load<EasyPostAddressValidatorSettings>();
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
