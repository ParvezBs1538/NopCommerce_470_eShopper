using System;
using System.Net;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace NopStation.Plugin.Payments.Afterpay
{
    public static class AfterpayPaymentHelper
    {
        public static WebRequest AddHeaders(this WebRequest request, string method, AfterpayPaymentSettings settings, string storeLocation)
        {
            var userAgent = string.Format("NopStation-afterpay-payment-method; Merchant/{0}) {1}", settings.MerchantId, storeLocation);
            request.Method = method;
            request.ContentType = "application/json";

            var autorization = settings.MerchantId + ":" + settings.MerchantKey;
            var binaryAuthorization = Encoding.UTF8.GetBytes(autorization);
            autorization = Convert.ToBase64String(binaryAuthorization);
            autorization = "Basic " + autorization;

            request.Headers.Add(HeaderNames.Authorization, autorization);
            request.Headers.Add(HeaderNames.Accept, "application/json");
            request.Headers.Add(HeaderNames.UserAgent, userAgent);

            return request;
        }
    }
}