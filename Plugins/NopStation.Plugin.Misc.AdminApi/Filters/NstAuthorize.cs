using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AdminApi.Extensions;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Filters;

public class NstAuthorizeAttribute : TypeFilterAttribute
{
    #region Ctor

    public NstAuthorizeAttribute() : base(typeof(NstAuthorize))
    {

    }

    #endregion

    #region Nested filter

    public class NstAuthorize : IAsyncActionFilter
    {
        protected virtual async Task<bool> ParseNstAuthorizationHeaderAsync(ActionExecutingContext actionContext)
        {
            var storeContext = NopInstance.Load<IStoreContext>();
            var httpContext = actionContext.HttpContext;

            httpContext.Request.Headers.TryGetValue(AdminApiCustomerDefaults.NSTHeader, out var keyFound);
            var requestKey = keyFound.FirstOrDefault();
            try
            {
                var settingService = NopInstance.Load<ISettingService>();
                var storeService = NopInstance.Load<IStoreService>();
                var workContext = NopInstance.Load<IWorkContext>();
                var storeScope = 0;
                var allStores = await storeService.GetAllStoresAsync();
                if (allStores.Count < 2)
                    storeScope = 0;
                else
                {
                    var currentStore = await storeContext.GetCurrentStoreAsync();
                    var storeId = currentStore.Id;
                    var store = await storeService.GetStoreByIdAsync(storeId);
                    storeScope = store?.Id ?? 0;
                }

                var adminApiSettings = await settingService.LoadSettingAsync<AdminApiSettings>(storeScope);

                var tokens = JwtHelper.JwtDecoder.DecodeToObject(requestKey, adminApiSettings.TokenSecret, true);
                if (tokens == null)
                    return false;

                if (tokens[AdminApiCustomerDefaults.NSTKey].ToString() != adminApiSettings.TokenKey)
                    return false;

                if (!adminApiSettings.CheckIat)
                    return true;

                if (long.TryParse(tokens[AdminApiCustomerDefaults.IAT].ToString(), out var createTimeStamp))
                {
                    var currentTimeStamp = ConvertToTimestamp(DateTime.UtcNow);
                    var leastTimeStamp = ConvertToTimestamp(DateTime.UtcNow.AddSeconds(-adminApiSettings.TokenSecondsValid));

                    return createTimeStamp <= currentTimeStamp && createTimeStamp >= leastTimeStamp;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static long ConvertToTimestamp(DateTime value)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var elapsedTime = value - epoch;
            return (long)elapsedTime.TotalSeconds;
        }

        private static async Task CreateNstAccessResponseMessageAsync(ActionExecutingContext actionContext)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList = [
                    await localizationService.GetResourceAsync("NopStation.AdminApi.Response.InvalidJwtToken")
                ]
            };

            actionContext.Result = new BadRequestObjectResult(response);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var adminApiSettings = NopInstance.Load<AdminApiSettings>();
            if (!adminApiSettings.EnableJwtSecurity)
            {
                await next();
                return;
            }

            var identity = await ParseNstAuthorizationHeaderAsync(context);
            if (!identity)
            {
                await CreateNstAccessResponseMessageAsync(context);
                return;
            }

            await next();
        }
    }

    #endregion
}
