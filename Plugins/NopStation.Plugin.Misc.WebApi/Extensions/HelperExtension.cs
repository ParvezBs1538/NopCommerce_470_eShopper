using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace NopStation.Plugin.Misc.WebApi.Extensions
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

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var characterArray = chars.Distinct().ToArray();
            var bytes = new byte[length * 8];
            var result = new char[length];
            using (var cryptoProvider = RandomNumberGenerator.Create())
            {
                cryptoProvider.GetBytes(bytes);
            }
            for (var i = 0; i < length; i++)
            {
                var value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }
    }
}
