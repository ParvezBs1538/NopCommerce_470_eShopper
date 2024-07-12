using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Nop.Plugin.NopStation.WebApi.Extensions
{
    public static class HelperExtension
    {
        public static Guid GetGuid(string deviceId)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(deviceId));
                var result = new Guid(hash);
                return result;
            }
        }

        public static string GetAppDeviceId(this HttpRequest httpRequest)
        {
            if (httpRequest.Headers.TryGetValue(WebApiCustomerDefaults.DeviceId, out StringValues headerValues))
            {
                var deviceId = headerValues.FirstOrDefault();
                if (deviceId != null)
                    return deviceId;
            }
            return string.Empty;
        }
    }
}
