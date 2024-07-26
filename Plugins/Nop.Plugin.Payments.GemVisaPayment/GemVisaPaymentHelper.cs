using System;
using System.Net;
using System.Text;
using Microsoft.Net.Http.Headers;


namespace Nop.Plugin.Payments.GemVisaPayment
{
    public static class GemVisaPaymentHelper
    {
        public static WebRequest AddHeaders(WebRequest request, string method, GemVisaPaymentSettings settings)
        {
            request.Method = method;
            request.ContentType = "application/json";
            //request.Headers.Add(HeaderNames.Accept, "application/json");
            var autorization = settings.MerchantId + ":" + settings.SecretKey;
            var binaryAuthorization = Encoding.UTF8.GetBytes(autorization);
            autorization = Convert.ToBase64String(binaryAuthorization);
            autorization = "Basic " + autorization;

            request.Headers.Add(HeaderNames.Authorization, autorization);

            return request;
        }
    }
}
