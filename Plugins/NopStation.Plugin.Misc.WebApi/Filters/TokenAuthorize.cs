using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Extensions;

namespace NopStation.Plugin.Misc.WebApi.Filters
{
    public class TokenAuthorizeAttribute : TypeFilterAttribute
    {
        #region Ctor

        public TokenAuthorizeAttribute() : base(typeof(TokenAuthorizeAttributeFilter))
        {
        }

        #endregion

        #region Nested class

        public class TokenAuthorizeAttributeFilter : IAuthorizationFilter
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
                bool check = true;

                if (actionContext.HttpContext.Request.Headers.TryGetValue(WebApiCustomerDefaults.Token, out StringValues checkToken))
                {
                    var token = checkToken.FirstOrDefault();
                    var webApiSettings = NopInstance.Load<WebApiSettings>();
                    try
                    {
                        var payload = JwtHelper.JwtDecoder.DecodeToObject(token, webApiSettings.SecretKey, true);
                        check = true;
                    }
                    catch
                    {
                        check = false;
                    }
                }

                return check;
            }

            private void Challenge(AuthorizationFilterContext actionContext)
            {
                var localizationService = NopInstance.Load<ILocalizationService>();
                var response = new BaseResponseModel
                {
                    ErrorList = new List<string>
                    {
                        localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidToken").Result
                    }
                };

                actionContext.Result = new ObjectResult(response)
                {
                    StatusCode = 403
                };

                return;
            }
        }

        #endregion
    }
}