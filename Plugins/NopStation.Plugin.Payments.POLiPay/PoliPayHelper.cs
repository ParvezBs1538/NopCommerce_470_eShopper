using System;
using System.Net;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace NopStation.Plugin.Payments.POLiPay
{
    public static class PoliPayHelper
    {
        public static WebRequest AddHeaders(WebRequest request, string method, PoliPaySettings settings)
        {
            request.Method = method;
            var autorization = settings.MerchantCode + ":" + settings.AuthCode;
            var binaryAuthorization = Encoding.UTF8.GetBytes(autorization);
            autorization = Convert.ToBase64String(binaryAuthorization);
            autorization = "Basic " + autorization;

            request.Headers.Add(HeaderNames.Authorization, autorization);
            request.Headers.Add(HeaderNames.ContentType, "application/json");

            return request;
        }
    }
}
