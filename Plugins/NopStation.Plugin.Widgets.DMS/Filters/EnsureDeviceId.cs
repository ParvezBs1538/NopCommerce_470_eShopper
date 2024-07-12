using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Widgets.DMS.Extensions;

namespace NopStation.Plugin.Widgets.DMS.Filters
{
    public class EnsureDeviceIdAttribute : TypeFilterAttribute
    {
        #region Ctor

        public EnsureDeviceIdAttribute(bool ignore = false) : base(typeof(EnsureDeviceIdAttributeFilter))
        {
            IgnoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        /// <summary>
        /// Gets a value indicating whether to ignore the execution of filter actions
        /// </summary>
        public bool IgnoreFilter { get; }

        #endregion

        #region Nested class

        public class EnsureDeviceIdAttributeFilter : IAsyncAuthorizationFilter //IAuthorizationFilter
        {
            private readonly bool _ignoreFilter;

            public EnsureDeviceIdAttributeFilter(bool ignoreFilter)
            {
                _ignoreFilter = ignoreFilter;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext actionContext)
            {
                var (identity, error) = await ParseAuthorizationHeaderAsync(actionContext);
                if (!identity)
                {
                    Challenge(actionContext, error);
                    return;
                }
            }

            protected virtual async Task<(bool identity, string error)> ParseAuthorizationHeaderAsync(AuthorizationFilterContext actionContext)
            {
                //check whether this filter has been overridden for the action
                var actionFilter = actionContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter)
                    .OfType<EnsureDeviceIdAttribute>()
                    .FirstOrDefault();

                //ignore filter (the action is available even if a customer hasn't access to the admin area)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return (true, "");

                var request = actionContext.HttpContext.Request;
                var localizationService = NopInstance.Load<ILocalizationService>();
                var msg = await localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound");
                try
                {
                    var deviceId = request.GetAppDeviceId();
                    if (string.IsNullOrEmpty(deviceId))
                        return (false, await localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));
                    return (true, "");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }

            private void Challenge(AuthorizationFilterContext actionContext, string error = "")
            {
                var response = new BaseResponseModel
                {
                    ErrorList = new List<string>
                    {
                        error
                    }
                };

                actionContext.Result = new BadRequestObjectResult(response);
                return;
            }
        }

        #endregion
    }
}
