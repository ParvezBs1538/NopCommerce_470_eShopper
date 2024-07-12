using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Extensions;

namespace NopStation.Plugin.Misc.WebApi.Filters
{
    public class NstAuthorizeAttribute : TypeFilterAttribute
    {
        #region Ctor

        public NstAuthorizeAttribute() : base(typeof(NstAuthorize))
        {

        }

        #endregion

        #region Nested filter

        public class NstAuthorize : IActionFilter
        {
            public void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var webApiSettings = NopInstance.Load<WebApiSettings>();
                if (!webApiSettings.EnableJwtSecurity)
                    return;
                var identity = ParseNstAuthorizationHeader(filterContext);
                if (identity == false)
                {
                    CreateNstAccessResponceMessage(filterContext);
                    return;
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                //do nothing
            }

            protected virtual bool ParseNstAuthorizationHeader(ActionExecutingContext actionContext)
            {
                var storeContext = NopInstance.Load<IStoreContext>();
                var httpContext = NopInstance.Load<IHttpContextAccessor>().HttpContext;

                httpContext.Request.Headers.TryGetValue(WebApiCustomerDefaults.NST, out StringValues keyFound);
                var requestkey = keyFound.FirstOrDefault();
                try
                {
                    var settingService = NopInstance.Load<ISettingService>();
                    var storeService = NopInstance.Load<IStoreService>();
                    var workContext = NopInstance.Load<IWorkContext>();
                    var storeScope = 0;
                    if (storeService.GetAllStoresAsync().Result.Count < 2)
                        storeScope = 0;
                    else
                    {
                        var storeId = storeContext.GetCurrentStoreAsync().Result.Id;
                        var store = storeService.GetStoreByIdAsync(storeId).Result;
                        storeScope = store?.Id ?? 0;
                    }

                    var securitySettings = settingService.LoadSettingAsync<WebApiSettings>(storeScope).Result;

                    var tokens = JwtHelper.JwtDecoder.DecodeToObject(requestkey, securitySettings.TokenSecret, true);
                    if (tokens != null)
                    {
                        if (tokens[WebApiCustomerDefaults.NSTKey].ToString() != securitySettings.TokenKey)
                            return false;

                        if (!securitySettings.CheckIat)
                            return true;

                        if (long.TryParse(tokens[WebApiCustomerDefaults.IAT].ToString(), out var createTimeStamp))
                        {
                            var currentTimeStamp = ConvertToTimestamp(DateTime.UtcNow);
                            var leastTimeStamp = ConvertToTimestamp(DateTime.UtcNow.AddSeconds(-securitySettings.TokenSecondsValid));

                            return createTimeStamp <= currentTimeStamp && createTimeStamp >= leastTimeStamp;
                        }
                        return false;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }

            private long ConvertToTimestamp(DateTime value)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var elapsedTime = value - epoch;
                return (long)elapsedTime.TotalSeconds;
            }

            private void CreateNstAccessResponceMessage(ActionExecutingContext actionContext)
            {
                var localizationService = NopInstance.Load<ILocalizationService>();
                var response = new BaseResponseModel
                {
                    ErrorList = new List<string>
                    {
                        localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidJwtToken").Result
                    }
                };

                actionContext.Result = new BadRequestObjectResult(response);
                return;
            }
        }

        #endregion
    }
}
