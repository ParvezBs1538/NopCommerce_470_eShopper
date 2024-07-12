namespace NopStation.Plugin.Payments.StripePaymentElement;

public class StripeDefaults
{
    public static string AppleVerificationFilePath => "Plugins/NopStation.Plugin.Payments.StripePaymentElement/apple-developer-merchantid-domain-association-{0}";

    public static string PaymentIntentId => "Payment Intent Id";

    public static string SCRIPT_VIEW_COMPONENT_NAME => "StripePaymentElementScript";

    public static string SystemName => "NopStation.Plugin.Payments.StripePaymentElement";

    public static string[] Themes => new[] { "stripe", "night", "flat" };

    public static string[] Layouts => new[] { "tabs", "accordion" };
}
