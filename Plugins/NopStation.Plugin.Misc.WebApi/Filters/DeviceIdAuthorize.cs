using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Filters
{
    public class DeviceIdAuthorizeAttribute : TypeFilterAttribute
    {
        #region Ctor

        public DeviceIdAuthorizeAttribute() : base(typeof(DeviceIdAuthorizeAttributeFilter))
        {
        }

        #endregion

        #region Nested class

        public class DeviceIdAuthorizeAttributeFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext actionContext)
            {
                var identity = ParseAuthorizationHeader(actionContext);
                if (identity == false)
                {
                    Challenge(actionContext);
                    return;
                }
            }

            protected virtual bool ParseAuthorizationHeader(AuthorizationFilterContext actionContext)
            {
                if (actionContext.HttpContext.Request.Headers.TryGetValue(WebApiCustomerDefaults.DeviceId, out StringValues checkToken))
                {
                    var token = checkToken.FirstOrDefault();
                    return !string.IsNullOrWhiteSpace(token);
                }
                return false;
            }

            private void Challenge(AuthorizationFilterContext actionContext)
            {
                var localizationService = NopInstance.Load<ILocalizationService>();
                var response = new BaseResponseModel
                {
                    ErrorList = new List<string>
                    {
                        localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidDeviceId").Result
                    }
                };

                actionContext.Result = new BadRequestObjectResult(response);
                return;
            }
        }

        #endregion
    }
}