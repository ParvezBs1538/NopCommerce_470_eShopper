namespace NopStation.Plugin.Payments.Nagad
{
    public static class NagadPaymentDefaults
    {
        public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.Nagad";
        public static string SANDBOX_BASE_URL => "http://sandbox.mynagad.com:10080";
        public static string BASE_URL => "";
        public static string CHECKOUT_INITIALIZE_PATH => "/remote-payment-gateway-1.0/api/dfs/check-out/initialize/{0}/{1}";
        public static string CHECKOUT_COMPLETE_PATH => "/remote-payment-gateway-1.0/api/dfs/check-out/complete/{0}";
        public static string PAYMET_VERIFICATION_PATH => "/remote-payment-gateway-1.0/api/dfs/verify/payment/{0}";
        public static string PAYMET_VERIFICATION_FAILED => "Nagad payment verification failed for order {0}, Error code {1} and error message {2}";
        public static string PAYMET_INITIALIZATION_FAILED => "Nagad payment initialization failed for order {0}, Error code {1} and error message {2}";
        public static string PAYMET_ORDER_COMPLETE_FAILED => "Nagad payment order complete failed for order {0}, Error code {1} and error message {2}";
    }
}
