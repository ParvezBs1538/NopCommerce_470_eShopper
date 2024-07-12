using System;
using System.Text;

namespace NopStation.Plugin.Payments.OABIPayment.Helpers
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
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
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

        public static bool IsConfigured(OABIPaymentSettings settings)
        {
            //client id and secret are required to request services
            return !string.IsNullOrEmpty(settings?.TranPortalId) && !string.IsNullOrEmpty(settings?.TranPortaPassword) && !string.IsNullOrEmpty(settings?.ResourceKey);
        }
    }
}
