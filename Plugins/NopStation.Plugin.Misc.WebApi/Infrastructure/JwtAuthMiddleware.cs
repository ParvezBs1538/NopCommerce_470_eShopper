using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.NopStation.WebApi.Models.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public JwtAuthMiddleware(RequestDelegate next,
            ILogger logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IWorkContext workContext, IWebHelper webHelper,
            ICustomerService customerService, ICustomerApiService customerApiService, WebApiSettings webApiSettings)
        {
            string token;
            if (context.Request.Headers.TryGetValue(WebApiCustomerDefaults.Token, out var tokenKey))
            {
                token = tokenKey.FirstOrDefault();
            }
            else
            {
                var cookieName = $".Nop.Customer.Token";
                token = context.Request?.Cookies[cookieName];

                if (string.IsNullOrWhiteSpace(token))
                    token = webHelper.QueryString<string>("customerToken");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                SetCustomerTokenCookie(context, token);
                try
                {
                    var load = JwtHelper.JwtDecoder.DecodeToObject(token, webApiSettings.SecretKey, true);
                    if (load != null)
                    {
                        var customerId = Convert.ToInt32(load[WebApiCustomerDefaults.CustomerId]);
                        var customer = await customerService.GetCustomerByIdAsync(customerId);
                        await workContext.SetCurrentCustomerAsync(customer);
                    }
                }
                catch (Exception ex)
                {
                    var localizationService = NopInstance.Load<ILocalizationService>();
                    var baseResponse = new BaseResponseModel();
                    baseResponse.ErrorList.Add(localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidToken").Result);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 403;
                    await _logger.ErrorAsync(ex.Message, ex);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(baseResponse));
                }
            }
            else if (context.Request.Headers.TryGetValue(WebApiCustomerDefaults.DeviceId, out var deviceIdKey))
            {
                var deviceId = deviceIdKey.FirstOrDefault();
                SetCustomerDeviceIdCookie(context, deviceId);
                var customerGuid = HelperExtension.GetGuid(deviceId);
                var customer = await customerService.GetCustomerByGuidAsync(customerGuid);
                if (customer != null && await customerService.IsRegisteredAsync(customer))
                {
                    customer.CustomerGuid = Guid.NewGuid();
                    await customerService.UpdateCustomerAsync(customer);
                    customer = await customerApiService.InsertDeviceGuestCustomerAsync(deviceId);
                }
                else if (customer == null)
                    customer = await customerApiService.InsertDeviceGuestCustomerAsync(deviceId);

                await workContext.SetCurrentCustomerAsync(customer);
            }

            await _next(context);
        }

        protected virtual void SetCustomerTokenCookie(HttpContext context, string token)
        {
            //delete current cookie value
            var cookieName = $".Nop.Customer.Token";
            context.Response.Cookies.Delete(cookieName);

            //get date of cookie expiration
            var cookieExpires = 24 * 365; //TODO make configurable
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            //if passed guid is empty set cookie as expired
            if (string.IsNullOrWhiteSpace(token))
                cookieExpiresDate = DateTime.Now.AddMonths(-1);

            //set new cookie value
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            context.Response.Cookies.Append(cookieName, token, options);
        }

        protected virtual void SetCustomerDeviceIdCookie(HttpContext context, string deviceId)
        {
            //delete current cookie value
            var cookieName = $".Nop.Customer.DeviceId";
            context.Response.Cookies.Delete(cookieName);

            //get date of cookie expiration
            var cookieExpires = 24 * 365; //TODO make configurable
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            //if passed guid is empty set cookie as expired
            if (string.IsNullOrWhiteSpace(deviceId))
                cookieExpiresDate = DateTime.Now.AddMonths(-1);

            //set new cookie value
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            context.Response.Cookies.Append(cookieName, deviceId, options);
        }
    }
}