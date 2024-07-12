using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Filters;

/// <summary>
/// Represents filter attribute that validates IP address
/// </summary>
public sealed class ValidateIpAddressAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    public ValidateIpAddressAttribute() : base(typeof(ValidateIpAddressFilter))
    {
    }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that validates IP address
    /// </summary>
    private class ValidateIpAddressFilter : IAsyncActionFilter
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly SecuritySettings _securitySettings;

        #endregion

        #region Ctor

        public ValidateIpAddressFilter(IWebHelper webHelper,
            SecuritySettings securitySettings)
        {
            _webHelper = webHelper;
            _securitySettings = securitySettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        private async Task<bool> ValidateIpAddressAsync(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.HttpContext.Request == null)
                return true;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return true;

            //get action and controller names
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;

            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return true;

            //get allowed IP addresses
            var ipAddresses = _securitySettings.AdminAreaAllowedIpAddresses;

            //there are no restrictions
            if (ipAddresses == null || ipAddresses.Count == 0)
                return true;

            //whether current IP is allowed
            var currentIp = _webHelper.GetCurrentIpAddress();

            if (ipAddresses.Any(ip => ip.Equals(currentIp, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            //redirect to 'Access denied' page
            var localizationService = NopInstance.Load<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList =
                [
                    await localizationService.GetResourceAsync("NopStation.AdminApi.Response.IpRestricted")
                ]
            };

            context.Result = new ObjectResult(response) { StatusCode = 403 };
            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">A context for action filters</param>
        /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var valid = await ValidateIpAddressAsync(context);
            if (valid)
                await next();
        }

        #endregion
    }

    #endregion
}