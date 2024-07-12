namespace NopStation.Plugin.Payments.Unzer
{
    public class UnzerPaymentDefaults
    {
        public const string VIEW_COMPONENT_NAME = "Unzer";

        public static string SystemName => "NopStation.Plugin.Payments.Unzer";

        public static string OnePageCheckoutRouteName => "CheckoutOnePage";

        public static string PaymentResourceIDKey => "Provided Resource Id";

        public static string CustomerIdAttribute => "UnzerCustomerId";

        public static string PaymentIdAttribute => "UnzerPaymentId";

        public static string RedirectUrlAttribute => "UnzerRedirectUrl";

        public static string RedirectionUrlLastPart => $"UnzerPayment/CompleteChargeAndPayment?success=(payment.isSuccess)&uuid=(payment.processing.uniqueId)";

        public static string CardPaymentPrefix => "crd";

        public static string PaypalPaymentPrefix => "ppl";

        public static string SofortPaymentPrefix => "sft";

        public static string EpsPaymentPrefix => "eps";

        public static string SCRIPT_VIEW_COMPONENT_NAME => "UnzerScript";
    }
}