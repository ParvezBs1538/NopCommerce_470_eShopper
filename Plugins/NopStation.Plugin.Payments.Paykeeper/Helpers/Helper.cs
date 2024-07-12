using System;
using System.Security.Cryptography;
using System.Text;

namespace NopStation.Plugin.Payments.Paykeeper.Helpers
{
    public class Helper
    {
        public static string GetBase64String(string str)
        {
            return Convert.ToBase64String(HexStringToHex(GetHexString(str)));
        }

        public static string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private static string GetHexString(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var valueHexString = BitConverter.ToString(buffer);
            valueHexString = valueHexString.Replace("-", "");
            return valueHexString;
        }

        private static byte[] HexStringToHex(string inputHex)
        {
            var resultantArray = new byte[inputHex.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }


    }
}
