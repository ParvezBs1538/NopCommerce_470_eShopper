namespace NopStation.Plugin.Payments.NetsEasy
{
    public static class NetsEasyPaymentDefaults
    {
        public static string PluginSystemName => "NopStation.Plugin.Payments.NetsEasy";

        public static string TestUrl => "https://test.api.dibspayment.eu";

        public static string TestCheckoutScriptUrl => "https://test.checkout.dibspayment.eu/v1/checkout.js?v=1";

        public static string LiveUrl => "https://api.dibspayment.eu";

        public static string LiveCheckoutScriptUrl => "https://checkout.dibspayment.eu/v1/checkout.js?v=1";

        public static string PaymentRequestSessionKey => "OrderPaymentInfo";

        public const string PAYMENT_INFO_VIEW_COMPONENT_NAME = "NetsEasyPaymentInfo";

        public const string SCRIPT_VIEW_COMPONENT_NAME = "NetsEasyPaymentScripts";
        public static string SynchronizationTask => "NopStation.Plugin.Payments.NetsEasy.Services.ChargeSynchronizationTask";
        public static int DefaultSynchronizationPeriod => 1;
        public static string SynchronizationTaskName => "Synchronization (NetsEasy plugin)";
    }
}
