using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Widgets.DMS.Extensions;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Filters
{
    public class TokenAuthorizeAttribute : TypeFilterAttribute
    {
        #region Ctor

        public TokenAuthorizeAttribute(bool ignore = false) : base(typeof(TokenAuthorizeAttributeFilter))
        {
            IgnoreFilter = ignore;
            Arguments = new object[] { ignore };
        }

        public bool IgnoreFilter { get; }

        #endregion

        #region Nested class

        public class TokenAuthorizeAttributeFilter : IAsyncAuthorizationFilter //IAuthorizationFilter
        {
            private readonly bool _ignoreFilter;

            public TokenAuthorizeAttributeFilter(bool ignoreFilter)
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
                var localizationService = NopInstance.Load<ILocalizationService>();

                //check whether this filter has been overridden for the action
                var actionFilter = actionContext.ActionDescriptor.FilterDescriptors
                    .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                    .Select(filterDescriptor => filterDescriptor.Filter)
                    .OfType<TokenAuthorizeAttribute>()
                    .FirstOrDefault();

                //ignore filter (the action is available even if a customer hasn't access to the admin area)
                if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                    return (true, null);

                var msg = await localizationService.GetResourceAsync("NopStation.DMS.Response.InvalidToken");

                var request = actionContext.HttpContext.Request;

                if (request.Headers.TryGetValue(DMSDefaults.Token, out StringValues checkToken))
                {
                    var token = checkToken.FirstOrDefault();
                    try
                    {
                        var payload = JwtHelper.JwtDecoder.DecodeToObject(token, DMSDefaults.CustomerSecret, true);
                        if (payload != null && payload.ContainsKey(DMSDefaults.CustomerId))
                        {

                            var customerId = Convert.ToInt32(payload[DMSDefaults.CustomerId]);
                            var customerService = NopInstance.Load<ICustomerService>();
                            var customer = await customerService.GetCustomerByIdAsync(customerId);

                            if (customer == null)
                                return (false, await localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

                            if (!await customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                                return (false, await localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));

                            string jwtDeviceId = (string)payload[DMSDefaults.DeviceId_Attribute];
                            var logger = NopInstance.Load<ILogger>();
                            var headerDeviceId = request.GetAppDeviceId();
                            if (headerDeviceId == null)
                                return (false, await localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.Header.DeviceId.NotFound"));


                            if (headerDeviceId != jwtDeviceId)
                            {
                                await logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Debug, "dms device info",
                                    JsonConvert.SerializeObject(payload) + "\n" + $"headerDeviceId : {headerDeviceId}\n" + $"jwtDeviceId : {jwtDeviceId}\n\n" + $"Token: {token}");

                                return (false, await localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.InvalidDeviceId"));
                            }

                            var shipperDeviceService = NopInstance.Load<IShipperDeviceService>();
                            var currentCustomerDevice = await shipperDeviceService.GetShipperDeviceByCustomerAsync(customer);
                            if (currentCustomerDevice == null)
                                return (false, await localizationService.GetResourceAsync("NopStation.DMS.Account.DeviceNotRegistered"));

                            if (currentCustomerDevice.DeviceToken != jwtDeviceId)
                                return (false, await localizationService.GetResourceAsync("NopStation.DMS.ShipperDevice.NotValid"));

                            return (true, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, ex.Message);
                    }
                }

                return (false, msg);
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