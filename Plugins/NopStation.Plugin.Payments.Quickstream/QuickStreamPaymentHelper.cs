using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NopStation.Plugin.Payments.Quickstream.Models;

namespace NopStation.Plugin.Payments.Quickstream
{
    public static class QuickStreamPaymentHelper
    {
        public static string ToQueryString(SecurityTokenRequestBody securityTokenRequestBody)
        {
            var serializedRequest = JsonConvert.SerializeObject(securityTokenRequestBody);

            var deserializedToDictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(serializedRequest);

            var queryString = deserializedToDictionary.Select(x => HttpUtility.UrlEncode(x.Key) + "=" + HttpUtility.UrlEncode(x.Value));

            return string.Join("&", queryString);
        }

        public static WebRequest AddHeaders(WebRequest request, string method, string key)
        {
            request.Method = method;
            var autorization = key + ":" + string.Empty;
            var binaryAuthorization = Encoding.UTF8.GetBytes(autorization);
            autorization = Convert.ToBase64String(binaryAuthorization);
            autorization = "Basic " + autorization;

            request.Headers.Add(HeaderNames.Authorization, autorization);
            request.Headers.Add(HeaderNames.ContentType, "application/json");

            return request;
        }

        public static WebRequest AddHeadersWithoutContentType(WebRequest request, string method, string key)
        {
            request.Method = method;
            var autorization = key + ":" + string.Empty;
            var binaryAuthorization = Encoding.UTF8.GetBytes(autorization);
            autorization = Convert.ToBase64String(binaryAuthorization);
            autorization = "Basic " + autorization;

            request.Headers.Add(HeaderNames.Authorization, autorization);

            return request;
        }
    }
}
