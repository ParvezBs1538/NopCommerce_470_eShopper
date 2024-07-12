using System.Net;
using Microsoft.Net.Http.Headers;

namespace NopStation.Plugin.Payments.CBL
{
    public static class CBLPaymentHelper
    {
        public static WebRequest AddHeaders(this WebRequest request, string method)
        {
            request.Method = method;
            request.ContentType = "application/json";
            request.Headers.Add(HeaderNames.Accept, "application/json");

            return request;
        }
    }
}
