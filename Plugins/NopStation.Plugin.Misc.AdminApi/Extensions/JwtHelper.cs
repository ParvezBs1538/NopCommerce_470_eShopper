using System;
using System.Linq;
using System.Security.Cryptography;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace NopStation.Plugin.Misc.AdminApi.Extensions;

public class JwtHelper
{
    public static IJwtEncoder JwtEncoder
    {
        get
        {
            var algorithm = new HMACSHA256Algorithm();
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            return new JwtEncoder(algorithm, serializer, urlEncoder);
        }
    }

    public static IJwtDecoder JwtDecoder
    {
        get
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(serializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            return new JwtDecoder(serializer, validator, urlEncoder);
        }
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
