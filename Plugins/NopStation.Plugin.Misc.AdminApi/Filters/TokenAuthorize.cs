using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using NopStation.Plugin.Misc.AdminApi.Extensions;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Filters;

public class TokenAuthorizeAttribute : TypeFilterAttribute
{
    #region Ctor

    public TokenAuthorizeAttribute() : base(typeof(TokenAuthorizeAttributeFilter))
    {
    }

    #endregion

    #region Nested class

    public class TokenAuthorizeAttributeFilter : IAsyncAuthorizationFilter
    {
        protected virtual async Task<bool> ParseAuthorizationHeaderAsync(AuthorizationFilterContext actionContext)
        {
            if (!actionContext.HttpContext.Request.Headers.TryGetValue(AdminApiCustomerDefaults.TokenHeader, out var checkToken))
                return false;

            var token = checkToken.FirstOrDefault();
            try
            {
                var adminApiSettings = NopInstance.Load<AdminApiSettings>();
                var load = JwtHelper.JwtDecoder.DecodeToObject(token, adminApiSettings.SecretKey, true);

                if (load == null)
                    return false;

                var customerService = NopInstance.Load<ICustomerService>();
                var customerId = int.Parse(load[AdminApiCustomerDefaults.CustomerId].ToString());
                if (await customerService.GetCustomerByIdAsync(customerId) is var customer)
                {
                    var permissionService = NopInstance.Load<IPermissionService>();
                    return await permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel, customer);
                }
            }
            catch { }
            return false;
        }

        private static async Task ChallengeAsync(AuthorizationFilterContext actionContext)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList = [
                    await localizationService.GetResourceAsync("NopStation.AdminApi.Response.InvalidToken")
                ]
            };

            actionContext.Result = new ObjectResult(response) { StatusCode = 403 };
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var identity = await ParseAuthorizationHeaderAsync(context);
            if (!identity)
            {
                await ChallengeAsync(context);
                return;
            }
        }
    }

    #endregion
}