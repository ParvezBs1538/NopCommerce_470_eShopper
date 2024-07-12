using System;

namespace NopStation.Plugin.Payments.iPayBd
{
    public class IpayDefaults
    {
        public static string SandboxUrl = "https://demo.ipay.com.bd/api/pg";
        public static string ProductionUrl = "https://app.ipay.com.bd/api/pg";

        public static Uri GetBaseUrl(bool sandbox)
        {
            return sandbox ? new Uri(SandboxUrl): new Uri(ProductionUrl);
        }
    }
}
