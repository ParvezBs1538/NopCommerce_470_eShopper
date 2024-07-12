using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Widgets.DMS.Extensions;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IWorkContext workContext, IWebHelper webHelper,
            ICustomerService customerService)
        {
            string token;
            if (context.Request.Headers.TryGetValue(DMSDefaults.Token, out var tokenKey))
            {
                token = tokenKey.FirstOrDefault();
            }
            else
            {
                var cookieName = $".Nop.Customer.DMS-Token";
                token = context.Request?.Cookies[cookieName];

                if (string.IsNullOrWhiteSpace(token))
                    token = webHelper.QueryString<string>("customerToken");
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                SetCustomerTokenCookie(context, token);
                var load = JwtHelper.JwtDecoder.DecodeToObject(token, DMSDefaults.CustomerSecret, true);
                if (load != null && load.ContainsKey(DMSDefaults.CustomerId))
                {
                    var customerId = Convert.ToInt32(load[DMSDefaults.CustomerId]);
                    var customer = await customerService.GetCustomerByIdAsync(customerId);
                    await workContext.SetCurrentCustomerAsync(customer);
                }
            }

            await _next(context);
        }

        protected virtual void SetCustomerTokenCookie(HttpContext context, string token)
        {
            //delete current cookie value
            var cookieName = $".Nop.Customer.DMS-Token";
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
    }
}