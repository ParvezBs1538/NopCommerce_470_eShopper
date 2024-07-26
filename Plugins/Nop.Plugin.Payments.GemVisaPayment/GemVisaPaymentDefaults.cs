namespace Nop.Plugin.Payments.GemVisaPayment
{
    public static class GemVisaPaymentDefaults
    {
        public static string PLUGIN_SYSTEM_NAME => "Payments.GemVisaPayment";
        public static string ORDER_DETAILS_URL => "{0}orderdetails/{1}";
        public static string PLATFORM_TYPE_DIRECT => "direct";
        public static string PAYMENT_STATUS_COMPLETED => "completed";
        public static string SANDBOX_PURCHASE_REQUEST_URL => "https://api.test.latitudefinancial.com/v1/applybuy-checkout-service/purchase";
        public static string PURCHASE_REQUEST_URL => "https://api.latitudefinancial.com/v1/applybuy-checkout-service/purchase";
        public static string SANDBOX_VERIFY_URL => "https://api.test.latitudefinancial.com/v1/applybuy-checkout-service/purchase/verify";
        public static string VERIFY_URL => "https://api.latitudefinancial.com/v1/applybuy-checkout-service/purchase/verify";
        public static string SANDBOX_MESSAGE_URL => "https://develop.checkout.test.merchant-services-np.lfscnp.com/assets/content.js?platform=direct&merchantId={0}";
        public static string MESSAGE_URL => "https://checkout.latitudefinancial.com/assets/content.js?platform=direct&merchantId={0}";
        public static string GemVisaInstallmentWidgetZone => "GemVisaInstallmentWidgetZone";
    }
}
