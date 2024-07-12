namespace NopStation.Plugin.Payments.Afterpay
{
    public static class AfterpayPaymentDefaults
    {
        public static string Currency => "NZD";
        public static string PLUGIN_SYSTEM_NAME => "NopStation.Plugin.Payments.Afterpay";
        public static string ORDER_DETAILS_URL => "{0}orderdetails/{1}";
        public static string SANDBOX_BASE_URL => "https://api-sandbox.afterpay.com";
        public static string BASE_URL => "https://api.afterpay.com";
        public static string CHECKOUT_URL_RESOURCE => "/v2/checkouts";
        public static string AUTH_URL_RESOURCE => "/v2/payments/auth";
        public static string CONFIGURATION => "/v2/configuration";
        public static string FULL_CAPTURE_URL_RESOURCE => "/v2/payments/capture";
        public static string CAPTURED => "CAPTURED";
        public static string GET_CHECKOUT => "/v2/checkouts/{0}";
        public static string GET_PAYMENT => "/v2/payments/token:{0}";
        public static string GET_REFUND => "{0}/v2/payments/{1}/refund";

        public static string APPROVED => "APPROVED";
        public static string AUTH_APPROVED => "AUTH_APPROVED";
        public static string DECLINED => "DECLINED";
        public static string CAPTURE_DECLINED => "CAPTURE_DECLINED";
        public static string AfterpayInstallmentWidgetZone => "AfterpayInstallmentWidgetZone";

        public static string PAYMENT_STATUS_UPDATE_TASK_TYPE => "NopStation.Plugin.Payments.Afterpay.AfterpayPaymentStatusUpdateTask";
    }
}