using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.AdminApi.Extensions;

namespace NopStation.Plugin.Misc.AdminApi.Infrastructure;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IWorkContext workContext,
        ICustomerService customerService,
        AdminApiSettings adminApiSettings)
    {
        string token;
        if (context.Request.Headers.TryGetValue(AdminApiCustomerDefaults.TokenHeader, out var tokenKey))
        {
            token = tokenKey.FirstOrDefault();
        }
        else
        {
            const string cookieName = $".Nop.Customer.AdminToken";
            token = context.Request?.Cookies[cookieName];
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            await _next(context);
            return;
        }

        try
        {
            SetCustomerTokenCookie(context, token);
            var load = JwtHelper.JwtDecoder.DecodeToObject(token, adminApiSettings.SecretKey, true);
            if (load != null)
            {
                var customerId = Convert.ToInt32(load[AdminApiCustomerDefaults.CustomerId]);
                var customer = await customerService.GetCustomerByIdAsync(customerId);
                await workContext.SetCurrentCustomerAsync(customer);
            }
        }
        catch
        {

        }

        await _next(context);
    }

    protected virtual void SetCustomerTokenCookie(HttpContext context, string token)
    {
        //delete current cookie value
        const string cookieName = $".Nop.Customer.AdminToken";
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
        const string cookieName = $".Nop.Customer.DeviceId";
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