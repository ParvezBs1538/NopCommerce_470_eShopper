namespace NopStation.Plugin.Payments.POLiPay
{
    public static class PoliPayDefaults
    {
        public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.POLiPay";
        public static string BASE_URL => "https://poliapi.apac.paywithpoli.com/api/v2/Transaction";
        public static string SANDBOX_BASE_URL => "https://poliapi.uat3.paywithpoli.com/api/v2/Transaction";
        public static string GET_URL_RESOURCE => "/Initiate";
        public static string GET_STATUS_RESOURCE => "/GetTransaction?token={0}";
        public static string ACCEPTED => "EulaAccepted";
        public static string COMPLETED => "Completed";
        public static string CANCELLD => "Cancelled";
    }
}
