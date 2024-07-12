namespace NopStation.Plugin.Payments.StripeWallet
{
    public class StripeDefaults
    {
        public static string AppleVerificationFilePath => "Plugins/NopStation.Plugin.Payments.StripeWallet/apple-developer-merchantid-domain-association-{0}";
        public static string PaymentIntentId => "Payment Intent Id";
        public static string SCRIPT_VIEW_COMPONENT_NAME => "StripeWalletScript";
        public static string SystemName => "NopStation.Plugin.Payments.StripeWallet";
    }
}
